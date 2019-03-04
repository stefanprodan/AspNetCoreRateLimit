using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit
{
    public class ClientRateLimitAsyncActionFilter : RateLimitMiddleware<ClientRateLimitProcessor>, IAsyncActionFilter
    {
        private readonly ILogger<ClientRateLimitMiddleware> _logger;

        public ClientRateLimitAsyncActionFilter(
            IOptions<ClientRateLimitOptions> options,
            IRateLimitCounterStore counterStore,
            IClientPolicyStore policyStore,
            IRateLimitConfiguration config,
            ILogger<ClientRateLimitMiddleware> logger)
        : base(options?.Value, new ClientRateLimitProcessor(options?.Value, counterStore, policyStore, config), config)
        {
            _logger = logger;
        }

        public Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var rateLimitAttribute = context.ActionDescriptor.EndpointMetadata
                                        .OfType<ClientRateLimitAttribute>()
                                        .FirstOrDefault();

            var rateLimitRule = base.GetDeclaredRule(context.HttpContext, rateLimitAttribute);

            return base.ThrottleAsync(context.HttpContext, () => next(), rateLimitRule);
        }

        protected override void LogBlockedRequest(HttpContext httpContext, ClientRequestIdentity identity, RateLimitCounter counter, RateLimitRule rule)
        {
            _logger?.LogInformation($"Request {identity.HttpVerb}:{identity.Path} from ClientId {identity.ClientId} has been blocked, quota {rule.Limit}/{rule.Period} exceeded by {counter.TotalRequests}. Blocked by rule {rule.Endpoint}, TraceIdentifier {httpContext.TraceIdentifier}.");
        }
    }
}