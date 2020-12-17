using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreRateLimit
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddMemoryCacheStores(this IServiceCollection services)
        {
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IClientPolicyStore, MemoryCacheClientPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IProcessingStrategyFactory, ProcessingStrategyFactory>();
            return services;
        }

        public static IServiceCollection AddDistributedCacheStores(this IServiceCollection services)
        {
            services.AddSingleton<IIpPolicyStore, DistributedCacheIpPolicyStore>();
            services.AddSingleton<IClientPolicyStore, DistributedCacheClientPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, DistributedCacheRateLimitCounterStore>();
            return services;
        }

        public static IServiceCollection AddStackExchangeRedisStores(this IServiceCollection services)
        {
            services.AddSingleton<IProcessingStrategyFactory, ProcessingStrategyFactory>();
            services.AddSingleton<IIpPolicyStore, StackExchangeRedisIpPolicyStore>();
            services.AddSingleton<IClientPolicyStore, StackExchangeRedisClientPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, StackExchangeRedisRateLimitCounterStore>();
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