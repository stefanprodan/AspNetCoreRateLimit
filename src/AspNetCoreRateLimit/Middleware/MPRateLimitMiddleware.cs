using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetCoreRateLimit
{
    class MPRateLimitMiddleware : RateLimitMiddleware<MPRateLimitProcessor>
    {
        private readonly ILogger<MPRateLimitMiddleware> _logger;

        public MPRateLimitMiddleware(RequestDelegate next,
            IOptions<MPRateLimitOptions> options,
            IRateLimitCounterStore counterStore,
            IMPPolicyStore policyStore,
            IRateLimitConfiguration config,
            ILogger<MPRateLimitMiddleware> logger)
        : base(next, options?.Value, new MPRateLimitProcessor(options?.Value, counterStore, policyStore, config), config)

        {
            _logger = logger;
        }

        protected override void LogBlockedRequest(HttpContext httpContext, ClientRequestIdentity identity, RateLimitCounter counter, RateLimitRule rule)
        {
            _logger.LogInformation($"Request {identity.HttpVerb}:{identity.Path} from IP {identity.ClientIp} has been blocked, quota {rule.Limit}/{rule.Period} exceeded by {counter.TotalRequests}. Blocked by rule {rule.Endpoint}, TraceIdentifier {httpContext.TraceIdentifier}.");
        }

    }
}
