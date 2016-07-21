using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
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

        public RateLimitMiddleware(RequestDelegate next, IOptions<RateLimitOptions> options)
        {
            _next = next;
            _options = options.Value;
        }

        public async Task Invoke(HttpContext context)
        {
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
