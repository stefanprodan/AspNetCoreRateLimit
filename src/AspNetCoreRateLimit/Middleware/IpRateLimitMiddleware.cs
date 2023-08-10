using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetCoreRateLimit
{
    public class IpRateLimitMiddleware : RateLimitMiddleware<IpRateLimitProcessor>
    {
        private readonly ILogger<IpRateLimitMiddleware> _logger;

        public IpRateLimitMiddleware(RequestDelegate next,
            IProcessingStrategy processingStrategy,
            IOptions<IpRateLimitOptions> options,
            IIpPolicyStore policyStore,
            IRateLimitConfiguration config,
            ILogger<IpRateLimitMiddleware> logger
        )
            : base(next, options?.Value, new IpRateLimitProcessor(options?.Value, policyStore, processingStrategy), config, logger)
        {
            _logger = logger;
        }

        protected override void LogBlockedRequest(HttpContext httpContext, ClientRequestIdentity identity, RateLimitCounter counter, RateLimitRule rule)
        {
            _logger.LogInformation($"Request {identity.HttpVerb}:{identity.Path} from IP {identity.ClientIp} has been blocked, quota {rule.Limit}/{rule.Period} exceeded by {counter.Count - rule.Limit}. Blocked by rule {rule.Endpoint}, TraceIdentifier {httpContext.TraceIdentifier}. MonitorMode: {rule.MonitorMode}");
        }
    }
}