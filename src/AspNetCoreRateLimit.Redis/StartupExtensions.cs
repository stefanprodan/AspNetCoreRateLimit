using System;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreRateLimit.Redis
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddRedisRateLimiting(this IServiceCollection services, Action<RedisRateLimitConfiguration> setupAction = null)
        {
            services.AddOptions();
            if (setupAction != null)
            {
                services.Configure(setupAction);
            }
            services.AddDistributedRateLimiting<RedisProcessingStrategy>();
            return services;
        }
    }
}