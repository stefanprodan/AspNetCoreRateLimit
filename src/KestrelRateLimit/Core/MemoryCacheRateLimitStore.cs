using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KestrelRateLimit
{
    public class MemoryCacheRateLimitStore : IRateLimitStore
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheRateLimitStore(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public void SaveCounter(string id, RateLimitCounter counter, TimeSpan expirationTime)
        {
            _memoryCache.Set(id, counter, new MemoryCacheEntryOptions().SetAbsoluteExpiration(expirationTime));
        }

        public bool CounterExists(string id)
        {
            RateLimitCounter stored;
            return _memoryCache.TryGetValue(id, out stored);
        }        

        public RateLimitCounter? GetCounter(string id)
        {
            RateLimitCounter stored;
            if (_memoryCache.TryGetValue(id, out stored))
            {
                return stored;
            }

            return null;
        }

        public void RemoveCounter(string id)
        {
            _memoryCache.Remove(id);
        }

        public void SaveOptions(string id, RateLimitOptions options)
        {
            _memoryCache.Set(id, options);
        }

        public bool OptionsExists(string id)
        {
            RateLimitOptions options;
            return _memoryCache.TryGetValue(id, out options);
        }

        public RateLimitOptions GetOptions(string id)
        {
            RateLimitOptions options;
            _memoryCache.TryGetValue(id, out options);
            return options;
        }

        public void RemoveOptions(string id)
        {
            _memoryCache.Remove(id);
        }
    }
}
