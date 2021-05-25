using Microsoft.Extensions.Caching.Distributed;

namespace AspNetCoreRateLimit
{
    public class DistributedCacheRateLimitCounterStore : DistributedCacheRateLimitStore<RateLimitCounter?>, IRateLimitCounterStore
    {
        public DistributedCacheRateLimitCounterStore(IDistributedCache cache) : base(cache)
        {
        }
    }
}