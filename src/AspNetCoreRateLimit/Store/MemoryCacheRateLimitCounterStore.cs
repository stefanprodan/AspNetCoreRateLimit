using Microsoft.Extensions.Caching.Memory;
using System;

namespace AspNetCoreRateLimit
{
    public class MemoryCacheRateLimitCounterStore: IRateLimitCounterStore
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheRateLimitCounterStore(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public void Set(string id, RateLimitCounter counter, TimeSpan expirationTime)
        {
            _memoryCache.Set(id, counter, new MemoryCacheEntryOptions().SetAbsoluteExpiration(expirationTime));
        }

        public bool Exists(string id)
        {
            RateLimitCounter stored;
            return _memoryCache.TryGetValue(id, out stored);
        }

        public RateLimitCounter? Get(string id)
        {
            RateLimitCounter stored;
            if (_memoryCache.TryGetValue(id, out stored))
            {
                return stored;
            }

            return null;
        }

        public void Remove(string id)
        {
            _memoryCache.Remove(id);
        }
    }
}
