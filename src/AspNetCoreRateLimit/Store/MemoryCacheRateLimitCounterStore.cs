using Microsoft.Extensions.Caching.Memory;

namespace AspNetCoreRateLimit
{
    public class MemoryCacheRateLimitCounterStore : MemoryCacheRateLimitStore<RateLimitCounter?>, IRateLimitCounterStore
    {
        public MemoryCacheRateLimitCounterStore(IMemoryCache cache) : base(cache)
        {
        }
    }
}