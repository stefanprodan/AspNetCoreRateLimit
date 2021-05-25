using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreRateLimit.Redis
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddStackExchangeRedisRateLimiting(this IServiceCollection services)
        {
            services.AddDistributedRateLimiting<StackExchangeRedisProcessingStrategy>();
            return services;
        }
    }
}