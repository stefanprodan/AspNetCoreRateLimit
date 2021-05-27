using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreRateLimit.Redis
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddRedisRateLimiting(this IServiceCollection services)
        {
            services.AddDistributedRateLimiting<RedisProcessingStrategy>();
            return services;
        }
    }
}