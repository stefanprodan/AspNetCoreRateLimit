using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit
{
    public class IpRateLimitAsyncActionFilter : RateLimitMiddleware<IpRateLimitProcessor>, IAsyncActionFilter
    {
        private readonly ILogger<IpRateLimitMiddleware> _logger;

        public IpRateLimitAsyncActionFilter(
            IOptions<IpRateLimitOptions> options,
            IRateLimitCounterStore counterStore,
            IIpPolicyStore policyStore,
            IRateLimitConfiguration config,
            ILogger<IpRateLimitMiddleware> logger)
        : base(options?.Value, new IpRateLimitProcessor(options?.Value, counterStore, policyStore, config), config)
        {
            _logger = logger;
        }

        public Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var rateLimitAttribute = context.ActionDescriptor.EndpointMetadata
                                        .OfType<IpRateLimitAttribute>()
                                        .FirstOrDefault();

            var rateLimitRule = base.GetDeclaredRule(context.HttpContext, rateLimitAttribute);

            return base.ThrottleAsync(context.HttpContext, () => next(), rateLimitRule);
        }

        protected override void LogBlockedRequest(HttpContext httpContext, ClientRequestIdentity identity, RateLimitCounter counter, RateLimitRule rule)
        {
            _logger?.LogInformation($"Request {identity.HttpVerb}:{identity.Path} from IP {identity.ClientIp} has been blocked, quota {rule.Limit}/{rule.Period} exceeded by {counter.TotalRequests}. Blocked by rule {rule.Endpoint}, TraceIdentifier {httpContext.TraceIdentifier}.");
        }
    }
}