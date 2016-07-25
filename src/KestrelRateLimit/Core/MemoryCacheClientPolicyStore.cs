using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

namespace KestrelRateLimit
{
    public class MemoryCacheClientPolicyStore: IClientPolicyStore
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheClientPolicyStore(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public void Set(string id, ClientRateLimitPolicy policy)
        {
            _memoryCache.Set(id, policy);
        }

        public bool Exists(string id)
        {
            ClientRateLimitPolicy stored;
            return _memoryCache.TryGetValue(id, out stored);
        }

        public ClientRateLimitPolicy Get(string id)
        {
            ClientRateLimitPolicy stored;
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
