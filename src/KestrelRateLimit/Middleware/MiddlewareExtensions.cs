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
        public static IApplicationBuilder UseIpRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<IpRateLimitMiddleware>();
        }

        public static IApplicationBuilder UseIpRateLimiting(this IApplicationBuilder builder, RateLimitOptions options)
        {
            return builder.UseMiddleware<IpRateLimitMiddleware>(new OptionsWrapper<RateLimitOptions>(options));
        }
    }
}
