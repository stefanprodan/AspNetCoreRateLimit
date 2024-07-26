using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace AspNetCoreRateLimit
{
    public class DistributedCacheRateLimitCounterStore : DistributedCacheRateLimitStore<RateLimitCounter?>, IRateLimitCounterStore
    {
        public DistributedCacheRateLimitCounterStore(IDistributedCache cache,ILogger<DistributedCacheRateLimitCounterStore> logger) : base(cache,logger)
        {
        }
    }
}