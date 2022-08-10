using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AspNetCoreRateLimit.Redis.BodyParameter.Core.ProcessingStrategies;
using AspNetCoreRateLimit.Redis.BodyParameter.Models;
using AspNetCoreRateLimit.Redis.BodyParameter.Store;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AspNetCoreRateLimit.Redis.BodyParameter;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public class ClientRateLimitAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var httpContextAccessor = validationContext.GetService<IHttpContextAccessor>();

        if (httpContextAccessor == null)
        {
            throw new ArgumentNullException(nameof(IHttpContextAccessor), "Don't forget to add service the IHttpContextAccessor.");
        }

        var clientBodyParameterOptions = validationContext.GetService<IOptions<ClientBodyParameterRateLimitOptions>>();

        if (clientBodyParameterOptions == null)
        {
            throw new ArgumentNullException(nameof(ClientBodyParameterRateLimitOptions));
        }

        var cacheClientBodyParameterPolicies = validationContext.GetService<IClientBodyParameterPolicyStore>();

        if (cacheClientBodyParameterPolicies == null)
        {
            throw new ArgumentNullException(nameof(IClientBodyParameterPolicyStore));
        }

        var request = httpContextAccessor.HttpContext?.Request;
        
        if (request == null)
        {
            throw new ArgumentNullException(nameof(HttpContext));
        }

        request.Headers.TryGetValue(clientBodyParameterOptions.Value.ClientIdHeader, out var clientId);

        if (string.IsNullOrEmpty(clientId)) return ValidationResult.Success;

        var clientBodyParameterRateLimitPolicy = cacheClientBodyParameterPolicies.Get($"{clientBodyParameterOptions.Value.ClientPolicyPrefix}_{clientId}");

        if (clientBodyParameterRateLimitPolicy == null)
        {
            throw new Exception($"Cannot found any rules for Client: {clientId}.");
        }
        
        var endpoint = $"{request.Method.ToLower()}:{request.Path}";

        var endpointRule = clientBodyParameterRateLimitPolicy.Rules.FirstOrDefault(x => x.Endpoint.Equals(endpoint, StringComparison.CurrentCultureIgnoreCase));

        if (endpointRule is not { EnableBodyParameter: true }) return ValidationResult.Success;

        var bodyParameterRule = endpointRule.BodyParameters.FirstOrDefault(x =>
            x.ParameterName.Equals(validationContext.MemberName, StringComparison.CurrentCultureIgnoreCase) &&
            x.ParameterValues.Any(y => y == (string)value!));

        if (bodyParameterRule == null)
        {
            throw new Exception($"Cannot found any parameter name or values: [{validationContext.MemberName}: {value}].");
        }
        
        var bodyParameterRedisProcessingStrategy = validationContext.GetService<BodyParameterRedisProcessingStrategy>();

        if (bodyParameterRedisProcessingStrategy == null)
        {
            throw new ArgumentNullException(nameof(BodyParameterRedisProcessingStrategy), "Don't forget to add service the '.AddDistributedBodyParameterRateLimitingStores()'.");
        }

        var key = BuildCounterKey(clientId, clientBodyParameterOptions.Value, endpointRule, bodyParameterRule);

        BodyParameterRateLimitCounter rateLimitCounter = bodyParameterRedisProcessingStrategy.ProcessRequest(key, bodyParameterRule);

        if (bodyParameterRule.Limit > 0)
        {
            if (rateLimitCounter.Timestamp + bodyParameterRule.PeriodTimespan.Value < DateTime.UtcNow)
            {
                // continue
            }
            else
            {
                // check if limit is reached
                if (rateLimitCounter.Count > bodyParameterRule.Limit)
                {
                    //compute retry after value
                    var retryAfter = RetryAfterFrom(rateLimitCounter.Timestamp, bodyParameterRule);

                    if (!endpointRule.MonitorMode)
                    {
                        // break execution
                        var responseErrorMessage = ReturnQuotaExceededResponse(httpContextAccessor.HttpContext, clientBodyParameterOptions.Value, bodyParameterRule, retryAfter);

                        return new ValidationResult(responseErrorMessage);
                    }
                }
            }
        }
        // if limit is zero or less, block the request.
        else
        {
            if (!endpointRule.MonitorMode)
            {
                var responseErrorMessage = ReturnQuotaExceededResponse(httpContextAccessor.HttpContext, clientBodyParameterOptions.Value, bodyParameterRule, int.MaxValue.ToString(CultureInfo.InvariantCulture));

                return new ValidationResult(responseErrorMessage);
            }
        }

        return ValidationResult.Success;
    }

    #region Helpers

    private static string BuildCounterKey(string clientId, BodyParameterRateLimitOptions rateLimitOptions, BodyParameterRateLimitRule bodyParameterRateLimitRule, EndpointBodyParameterRateLimitRule endpointBodyParameterRateLimitRule)
    {
        var clientAndEndPointKey = $"{rateLimitOptions.RateLimitCounterPrefix}_{clientId}_{bodyParameterRateLimitRule.Endpoint}";
        var parameterKey = $"{endpointBodyParameterRateLimitRule.Period}_{endpointBodyParameterRateLimitRule.ParameterName}_{endpointBodyParameterRateLimitRule.ParameterValues}";
        using var algorithm = SHA1.Create();
        return $"{Convert.ToBase64String(algorithm.ComputeHash(Encoding.UTF8.GetBytes(clientAndEndPointKey)))}:{Convert.ToBase64String(algorithm.ComputeHash(Encoding.UTF8.GetBytes(parameterKey)))}";
    }

    private static string RetryAfterFrom(DateTime timestamp, EndpointBodyParameterRateLimitRule rule)
    {
        var diff = timestamp + rule.PeriodTimespan.Value - DateTime.UtcNow;
        var seconds = Math.Max(diff.TotalSeconds, 1);
        return $"{seconds:F0}";
    }

    private static string FormatPeriodTimespan(TimeSpan period)
    {
        var sb = new StringBuilder();

        if (period.Days > 0)
        {
            sb.Append($"{period.Days}d");
        }

        if (period.Hours > 0)
        {
            sb.Append($"{period.Hours}h");
        }

        if (period.Minutes > 0)
        {
            sb.Append($"{period.Minutes}m");
        }

        if (period.Seconds > 0)
        {
            sb.Append($"{period.Seconds}s");
        }

        if (period.Milliseconds > 0)
        {
            sb.Append($"{period.Milliseconds}ms");
        }

        return sb.ToString();
    }

    private static string ReturnQuotaExceededResponse(HttpContext httpContext, ClientBodyParameterRateLimitOptions rateLimitOptions, EndpointBodyParameterRateLimitRule rule, string retryAfter)
    {
        var message = string.Format(
            rateLimitOptions.QuotaExceededResponse?.Content ??
            rateLimitOptions.QuotaExceededMessage ??
            "API parameter calls quota exceeded! maximum admitted {0} per {1}.",
            rule.Limit,
            rule.PeriodTimespan.HasValue ? FormatPeriodTimespan(rule.PeriodTimespan.Value) : rule.Period, retryAfter);
        if (!rateLimitOptions.DisableRateLimitHeaders)
        {
            httpContext.Response.Headers["Retry-After"] = retryAfter;
            httpContext.Response.Headers["X-Enable-Body-Parameter-Rate-Limit"] = "1";
        }

        return message;
    }

    #endregion
}