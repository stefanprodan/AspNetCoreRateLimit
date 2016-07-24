using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
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

            //save limits from options
            if (_options.ClientRateLimits != null && _options.ClientRateLimits.Any())
            {
                _processor.SaveClientRateLimits(_options.ClientRateLimits);
            }

            // compute identity from request
            var identity = SetIdentity(context);

            // check white list
            if (_processor.IsWhitelisted(identity))
            {
                await _next.Invoke(context);
                return;
            }

            var timeSpan = TimeSpan.FromSeconds(1);

            _processor.GetMatchingLimits(identity);

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
