using AspNetCoreRateLimit.Redis.BodyParameter.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace AspNetCoreRateLimit.Redis.BodyParameter.Store.DistributedCache
{
    public class DistributedCacheBodyParameterRateLimitCounterStore : DistributedCacheBodyParameterRateLimitStore<BodyParameterRateLimitCounter?>, IBodyParameterRateLimitCounterStore
    {
        public DistributedCacheBodyParameterRateLimitCounterStore(IDistributedCache cache) : base(cache)
        {
        }
    }
}