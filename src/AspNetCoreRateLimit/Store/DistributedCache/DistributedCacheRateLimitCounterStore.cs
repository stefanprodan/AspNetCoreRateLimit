using Microsoft.Extensions.Caching.Distributed;

namespace AspNetCoreRateLimit
{
    public class DistributedCacheRateLimitCounterStore(IDistributedCache cache) : DistributedCacheRateLimitStore<RateLimitCounter?>(cache), IRateLimitCounterStore
    {
    }
}