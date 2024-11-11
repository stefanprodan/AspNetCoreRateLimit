using Microsoft.Extensions.Caching.Memory;

namespace AspNetCoreRateLimit
{
    public class MemoryCacheRateLimitCounterStore(IMemoryCache cache) : MemoryCacheRateLimitStore<RateLimitCounter?>(cache), IRateLimitCounterStore
    {
    }
}