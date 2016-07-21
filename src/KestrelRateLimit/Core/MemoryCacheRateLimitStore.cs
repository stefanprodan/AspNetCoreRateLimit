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
            //RateLimitCounter stored;
            //if (!_memoryCache.TryGetValue(id, out stored))
            //{
                _memoryCache.Set(id, counter, new MemoryCacheEntryOptions().SetAbsoluteExpiration(expirationTime));
            //}
            //else
            //{
            //    _memoryCache.Set(id, counter);
            //}
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

        public void ClearCounters()
        {
            throw new NotImplementedException();
        }
    }
}
