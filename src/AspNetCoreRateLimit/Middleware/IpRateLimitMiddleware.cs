using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetCoreRateLimit
{
    public class IpRateLimitMiddleware : RateLimitMiddleware<IpRateLimitProcessor>
    {
        private readonly ILogger<IpRateLimitMiddleware> _logger;

        public IpRateLimitMiddleware(RequestDelegate next,
            IOptions<IpRateLimitOptions> options,
            IRateLimitCounterStore counterStore,
            IIpPolicyStore policyStore,
            IRateLimitConfiguration config,
            ILogger<IpRateLimitMiddleware> logger)
        : base(next, options?.Value, new IpRateLimitProcessor(options?.Value, counterStore, policyStore, config), config)

        {
            _logger = logger;
        }

        protected override void LogBlockedRequest(HttpContext httpContext, ClientRequestIdentity identity, RateLimitCounter counter, RateLimitRule rule)
        {
            _logger.LogInformation("Request {HttpVerb}:{Path} from IP {ClientIp} has been blocked, quota {Limit}/{Period} exceeded by {Count}. Blocked by rule {Endpoint}, TraceIdentifier {TraceIdentifier}.", identity.HttpVerb, identity.Path, identity.ClientIp, rule.Limit, rule.Period, counter.Count, rule.Endpoint, httpContext.TraceIdentifier);
        }
    }
}