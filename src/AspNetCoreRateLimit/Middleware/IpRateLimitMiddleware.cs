using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit
{
    public class IpRateLimitMiddleware : RateLimitMiddleware<IpRateLimitProcessor>
    {
        private readonly ILogger<IpRateLimitMiddleware> _logger;
        private readonly IIpAddressParser _ipParser;
        private readonly IpRateLimitProcessor _processor;
        private readonly IpRateLimitOptions _options;
        private readonly IIpPolicyStore _ipPolicyStore;

        public IpRateLimitMiddleware(RequestDelegate next, 
            IOptions<IpRateLimitOptions> options,
            IRateLimitCounterStore counterStore,
            IIpPolicyStore policyStore,
            IRateLimitConfiguration config,
            ILogger<IpRateLimitMiddleware> logger)
        : base(next, options?.Value, new IpRateLimitProcessor(options?.Value, counterStore, policyStore, config), config)

        {
            _logger = logger;
            _ipParser = ipParser != null ? ipParser : new ReversProxyIpParser(_options.RealIpHeader);
            _ipPolicyStore = policyStore;

            _processor = new IpRateLimitProcessor(_options, counterStore, policyStore, _ipParser);
        }

        protected override void LogBlockedRequest(HttpContext httpContext, ClientRequestIdentity identity, RateLimitCounter counter, RateLimitRule rule)
        {
            _logger.LogInformation($"Request {identity.HttpVerb}:{identity.Path} from IP {identity.ClientIp} has been blocked, quota {rule.Limit}/{rule.Period} exceeded by {counter.Count}. Blocked by rule {rule.Endpoint}, TraceIdentifier {httpContext.TraceIdentifier}.");
        }

        public virtual ClientRequestIdentity SetIdentity(HttpContext httpContext)
        {
            var clientId = "anon";
            if (httpContext.Request.Headers.Keys.Contains(_options.ClientIdHeader,StringComparer.CurrentCultureIgnoreCase))
            {
                clientId = httpContext.Request.Headers[_options.ClientIdHeader].First();
            }

            var clientIp = string.Empty;
            try
            {
                var ip = _ipParser.GetClientIp(httpContext);
                if(ip == null)
                {
                    throw new Exception("IpRateLimitMiddleware can't parse caller IP");
                }

                clientIp = ip.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("IpRateLimitMiddleware can't parse caller IP", ex);
            }

            return new ClientRequestIdentity
            {
                ClientIp = clientIp,
                Path = httpContext.Request.Path.ToString().ToLowerInvariant(),
                HttpVerb = httpContext.Request.Method.ToLowerInvariant(),
                ClientId = clientId
            };
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
            _logger.LogInformation($"Request {identity.HttpVerb}:{identity.Path} from IP {identity.ClientIp} has been blocked, quota {rule.Limit}/{rule.Period} exceeded by {counter.TotalRequests}. Blocked by rule {rule.Endpoint}, TraceIdentifier {httpContext.TraceIdentifier}.");
            if(counter.TotalRequests == (rule.Limit * _options.IpFloodWarningFactor))
            {
                _logger.LogWarning($"Flood tentative from IP Address {identity.ClientIp}");
            }
            else if (counter.TotalRequests == (rule.Limit * _options.IpFloodBanFactor))
            {
                _logger.LogWarning($"Too many Flood tentative from IP Address {identity.ClientIp}");
                TempBanIP(identity.ClientIp);
            }
        }

        private Task SetRateLimitHeaders(object rateLimitHeaders)
        {
            var headers = (RateLimitHeaders)rateLimitHeaders;

            headers.Context.Response.Headers["X-Rate-Limit-Limit"] = headers.Limit;
            headers.Context.Response.Headers["X-Rate-Limit-Remaining"] = headers.Remaining;
            headers.Context.Response.Headers["X-Rate-Limit-Reset"] = headers.Reset;

            return Task.CompletedTask;
        }

        private void TempBanIP(string IpAddress)
        {
            // prepare ban rule
            IpRateLimitPolicy banRule = new IpRateLimitPolicy
            {
                Ip = IpAddress,
                Rules = new List<RateLimitRule>(
                    new RateLimitRule[]
                    {
                        new RateLimitRule
                        {
                            Endpoint = "*",
                            Limit = 0,
                            Period = $"{_options.IpTempBanPeriod}"
                        }
                    })
            };

            // Get Policy Store
            var pol = _ipPolicyStore.Get($"{_options.IpPolicyPrefix}");

            // Check if IP is already banned
            foreach (IpRateLimitPolicy irlp in pol.IpRules)
                if (irlp.Ip == IpAddress)
                    foreach (RateLimitRule rlr in irlp.Rules)
                        if (rlr.Limit == 0)
                            return; // If is already banned no action needs to be performed

            // If is not already banned, add ban rule to set
            pol.IpRules.Add(banRule);
            _ipPolicyStore.Set(_options.IpPolicyPrefix, pol); // load updated set
            _logger.LogWarning($"IP Address {IpAddress} banned for {_options.IpTempBanPeriod}");
        }
    }
}