using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreRateLimit
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddInMemoryRateLimiting(this IServiceCollection services)
        {
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IClientPolicyStore, MemoryCacheClientPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
            return services;
        }

        public static IServiceCollection AddDistributedRateLimiting(this IServiceCollection services)
        {
            services.AddDistributedRateLimitingStores();
            services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
            return services;
        }

        public static IServiceCollection AddDistributedRateLimiting<T>(this IServiceCollection services)
            where T : class, IProcessingStrategy 
        {
            services.AddDistributedRateLimitingStores();
            services.AddSingleton<IProcessingStrategy, T>();
            return services;
        }

        private static IServiceCollection AddDistributedRateLimitingStores(this IServiceCollection services)
        {
            services.AddSingleton<IIpPolicyStore, DistributedCacheIpPolicyStore>();
            services.AddSingleton<IClientPolicyStore, DistributedCacheClientPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, DistributedCacheRateLimitCounterStore>();
            return services;
        }

        public static IApplicationBuilder UseIpRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<IpRateLimitMiddleware>();
        }

        public static IApplicationBuilder UseClientRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ClientRateLimitMiddleware>();
        }
    }
}