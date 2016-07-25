using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace KestrelRateLimit
{
    public class ClientRateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly ClientRateLimitProcessor _processor;

        private readonly ClientRateLimitOptions _options;

        public ClientRateLimitMiddleware(RequestDelegate next, 
            IOptions<ClientRateLimitOptions> options, 
            ILoggerFactory loggerFactory,
            IMemoryCache memoryCache = null
            )
        {
            _next = next;
            _options = options.Value;
            _logger = loggerFactory.CreateLogger<IpRateLimitMiddleware>();
            _memoryCache = memoryCache;

            _processor = new ClientRateLimitProcessor(_options, new MemoryCacheRateLimitCounterStore(_memoryCache), new MemoryCacheClientPolicyStore(_memoryCache));
        }

        public async Task Invoke(HttpContext context)
        {
            // check if rate limiting is enabled
            if (_options == null)
            {
                await _next.Invoke(context);
                return;
            }

            //save limits from options, for debug purposes only
            if (_options.ClientRules != null && _options.ClientRules.Any())
            {
                _processor.SaveClientRateLimits(_options.ClientRules);
            }

            // compute identity from request
            var identity = SetIdentity(context);

            // check white list
            if (_processor.IsWhitelisted(identity))
            {
                await _next.Invoke(context);
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
                        _logger.LogInformation($"Request {identity.HttpVerb}:{identity.Path} from ClienId {identity.ClientId} has been blocked, quota {rule.Limit}/{rule.Period} exceeded by {counter.TotalRequests}. Rule {rule.Endpoint}");

                        var message = string.IsNullOrEmpty(_options.QuotaExceededMessage) ? $"API calls quota exceeded! maximum admitted {rule.Limit} per {rule.Period}." : _options.QuotaExceededMessage;

                        // break execution
                        await QuotaExceededResponse(context, _options.HttpStatusCode, message, retryAfter);
                        return;
                    }
                }
            }

            //set X-Rate-Limit headers for the longest period
            if(rules.Any())
            {
                var rule = rules.OrderByDescending(x => x.PeriodTimespan.Value).First();
                var headers = _processor.GetRateLimitHeaders(identity, rule);
                headers.Context = context;

                context.Response.OnStarting(SetRateLimitHeaders, state: headers);
            }

            await _next.Invoke(context);
        }

        public virtual ClientRequestIdentity SetIdentity(HttpContext httpContext)
        {
            var clientId = "anon";
            if (httpContext.Request.Headers.Keys.Contains(_options.ClientIdHeader))
            {
                clientId = httpContext.Request.Headers[_options.ClientIdHeader].First();
            }

            return new ClientRequestIdentity
            {
                Path = httpContext.Request.Path.ToString().ToLowerInvariant(),
                HttpVerb = httpContext.Request.Method.ToLowerInvariant(),
                ClientId = clientId
            };
        }

        public virtual Task QuotaExceededResponse(HttpContext httpContext, int statusCode, string message, string retryAfter)
        {
            httpContext.Response.Headers["Retry-After"] = retryAfter;
            httpContext.Response.StatusCode = statusCode;
            return httpContext.Response.WriteAsync(message);
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
