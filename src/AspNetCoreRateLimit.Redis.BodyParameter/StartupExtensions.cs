using AspNetCoreRateLimit.Redis.BodyParameter.Core.ProcessingStrategies;
using AspNetCoreRateLimit.Redis.BodyParameter.Store;
using AspNetCoreRateLimit.Redis.BodyParameter.Store.DistributedCache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreRateLimit.Redis.BodyParameter
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddDistributedBodyParameterRateLimitingStores(this IServiceCollection services)
        {
            services.AddDistributedRateLimiting<RedisProcessingStrategy>();
            services.AddSingleton<IIpBodyParameterPolicyStore, DistributedCacheBodyParameterIpBodyParameterPolicyStore>();
            services.AddSingleton<IClientBodyParameterPolicyStore, DistributedCacheBodyParameterClientBodyParameterPolicyStore>();
            services.AddSingleton<IBodyParameterRateLimitCounterStore, DistributedCacheBodyParameterRateLimitCounterStore>();
            services.AddSingleton<BodyParameterRedisProcessingStrategy>();
            return services;
        }

        public static void AddBodyParameterRateLimitFilter(this MvcOptions options)
        {
            options.Filters.Add(typeof(RateLimitActionFilterAttribute));
        }
    }
}