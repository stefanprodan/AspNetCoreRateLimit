﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreRateLimit.Services;

namespace AspNetCoreRateLimit
{
    public class ClientRateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ClientRateLimitMiddleware> _logger;
        private readonly ClientRateLimitProcessor _processor;
        private readonly ClientRateLimitOptions _options;
        private IClientRequestStore _clientRequestStore;

        public ClientRateLimitMiddleware(RequestDelegate next,
            IOptions<ClientRateLimitOptions> options,
            IRateLimitCounterStore counterStore,
            IClientPolicyStore policyStore,
            IClientRequestStore clientRequestStore,
            ILogger<ClientRateLimitMiddleware> logger
            )
        {
            _next = next;
            _options = options.Value;
            _clientRequestStore = clientRequestStore;
            _logger = logger;

            _processor = new ClientRateLimitProcessor(_options, counterStore, policyStore);
        }

        public async Task Invoke(HttpContext httpContext)
        {
            // check if rate limiting is enabled
            if (_options == null)
            {
                await _next.Invoke(httpContext);
                return;
            }

            // compute identity from request
            var identity = await _clientRequestStore.GetClientRequestIdentityAsync(httpContext);

            // check white list
            if (_processor.IsWhitelisted(identity))
            {
                await _next.Invoke(httpContext);
                return;
            }

            var rules = _processor.GetMatchingRules(identity);

            foreach (var rule in rules)
            {
                if(rule.Limit > 0)
                {
                    // increment counter
                    var counter = _processor.ProcessRequest(identity, rule);

                    // check if key expired
                    if (counter.Timestamp + rule.PeriodTimespan.Value < DateTime.UtcNow)
                    {
                        continue;
                    }

                    // check if limit is reached
                    if (counter.TotalRequests > rule.Limit)
                    {
                        //compute retry after value
                        var retryAfter = _processor.RetryAfterFrom(counter.Timestamp, rule);

                        // log blocked request
                        LogBlockedRequest(httpContext, identity, counter, rule);
                  
                        // break execution
                        await ReturnQuotaExceededResponse(httpContext, rule, retryAfter);
                        return;
                    }
                }
                // if limit is zero or less, block the request.
                else
                {
                    // process request count
                    var counter = _processor.ProcessRequest(identity, rule);

                    // log blocked request
                    LogBlockedRequest(httpContext, identity, counter, rule);

                    // break execution (Int32 max used to represent infinity)
                    await ReturnQuotaExceededResponse(httpContext, rule, Int32.MaxValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    return;
                }
            }

            //set X-Rate-Limit headers for the longest period
            if(rules.Any() && !_options.DisableRateLimitHeaders)
            {
                var rule = rules.OrderByDescending(x => x.PeriodTimespan.Value).First();
                var headers = _processor.GetRateLimitHeaders(identity, rule);
                headers.Context = httpContext;

                httpContext.Response.OnStarting(SetRateLimitHeaders, state: headers);
            }

            await _next.Invoke(httpContext);
        }

        public virtual Task ReturnQuotaExceededResponse(HttpContext httpContext, RateLimitRule rule, string retryAfter)
        {
            var message = string.IsNullOrEmpty(_options.QuotaExceededMessage) ? $"API calls quota exceeded! maximum admitted {rule.Limit} per {rule.Period}." : _options.QuotaExceededMessage;

            if (!_options.DisableRateLimitHeaders)
            {
                httpContext.Response.Headers["Retry-After"] = retryAfter;
            }

            httpContext.Response.StatusCode = _options.HttpStatusCode;
            return httpContext.Response.WriteAsync(message);
        }

        public virtual void LogBlockedRequest(HttpContext httpContext, ClientRequestIdentity identity, RateLimitCounter counter, RateLimitRule rule)
        {
            _logger.LogInformation($"Request {identity.HttpVerb}:{identity.Path} from ClientId {identity.ClientId} has been blocked, quota {rule.Limit}/{rule.Period} exceeded by {counter.TotalRequests}. Blocked by rule {rule.Endpoint}, TraceIdentifier {httpContext.TraceIdentifier}.");
        }

        private Task SetRateLimitHeaders(object rateLimitHeaders)
        {
            var headers = (RateLimitHeaders)rateLimitHeaders;

            headers.Context.Response.Headers["X-Rate-Limit-Limit"] = headers.Limit;
            headers.Context.Response.Headers["X-Rate-Limit-Remaining"] = headers.Remaining;
            headers.Context.Response.Headers["X-Rate-Limit-Reset"] = headers.Reset;

            return Task.CompletedTask;
        }
    }
}
