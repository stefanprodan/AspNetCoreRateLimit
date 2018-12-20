using AspNetCoreRateLimit;
using AspNetCoreRateLimit.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace AspNetCoreRateLimit
{
    public class CustomIpRateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomIpRateLimitMiddleware> _logger;
        private readonly IpRateLimitOptions _options;
        private readonly IpRateLimitMiddleware _innerMiddleware;

        public CustomIpRateLimitMiddleware(RequestDelegate next, 
            IOptions<IpRateLimitOptions> options,
            IRateLimitCounterStore counterStore,
            IIpPolicyStore policyStore,
            ILogger<CustomIpRateLimitMiddleware> logger,
            ILogger<IpRateLimitMiddleware> innerLogger,
            IIpAddressParser ipParser = null
            )
        {
            _next = next;
            _options = options.Value;
            _logger = logger;
            _innerMiddleware = new IpRateLimitMiddleware(next, options, counterStore, policyStore, innerLogger, ipParser);
        }

        public async Task Invoke(HttpContext httpContext)
        {
            // check if rate limiting is enabled or if EndpointWhiteList has values ​​that meet the condition
            if (_options == null || SkipRateLimit(_options.EndpointWhitelist, httpContext.Request.Method, httpContext.Request.Path))
            {
                await _next.Invoke(httpContext);
                return;
            }

            await _innerMiddleware.Invoke(httpContext);
        }

        // Add wildcard matcher in EndpointWhiteList
        public static bool SkipRateLimit(IEnumerable<string> endpointWhiteList, string httpVerb, string path) =>
            endpointWhiteList.Any(x =>
                $"{httpVerb.ToLowerInvariant()}:{path.ToLowerInvariant()}".ToLowerInvariant().IsMatch(x.ToLowerInvariant()));
    }
}
