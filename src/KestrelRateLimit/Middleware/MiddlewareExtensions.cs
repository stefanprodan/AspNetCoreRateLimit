using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KestrelRateLimit
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitMiddleware>();
        }

        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder, RateLimitOptions options)
        {
            return builder.UseMiddleware<RateLimitMiddleware>(new OptionsWrapper<RateLimitOptions>(options));
        }
    }
}
