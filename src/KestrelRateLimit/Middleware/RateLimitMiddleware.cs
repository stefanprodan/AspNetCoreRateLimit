using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace KestrelRateLimit
{
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RateLimitOptions _options;
        private readonly ILogger _logger;
        private readonly IIPAddressParser _ipParser;

        public RateLimitMiddleware(RequestDelegate next, 
            IOptions<RateLimitOptions> options, 
            ILoggerFactory loggerFactory,
            IIPAddressParser ipParser = null
            )
        {
            _next = next;
            _options = options.Value;
            _logger = loggerFactory.CreateLogger<RateLimitMiddleware>();
            _ipParser = ipParser != null ? ipParser : new DefaultIpAddressParser();
        }

        public async Task Invoke(HttpContext context)
        {
            _logger.LogInformation($"Rate limiting request {context.Request.Path} id {context.TraceIdentifier}");

            var url = context.Request.GetDisplayUrl();
            var path = context.Request.Path;
            var httpMethod = context.Request.Method;
            var userHostAddress = context.Connection.RemoteIpAddress?.ToString();

            if (context.Request.Headers.Keys.Contains("X-Rate-Limited"))
            {
                await QuotaExceededResponse(context, 429, "Too Many Requests", "1 sec");
                return;
            }

            if (context.Request.Headers.Keys.Contains("X-Rate-Limit-Info"))
            {
                context.Response.OnStarting(SetRateLimitHeaders, state: new RateLimitHeaders { Context = context, Limit = "per day", Remaining = "200", Reset = "2016.07.21 12:30:59" });
            }

            await _next.Invoke(context);

            _logger.LogInformation($"Finished handling request {context.TraceIdentifier}");
        }

        protected virtual Task QuotaExceededResponse(HttpContext httpContext, int statusCode, string message, string retryAfter)
        {
            httpContext.Response.Headers["Retry-After"] = retryAfter;
            httpContext.Response.StatusCode = 429;
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
