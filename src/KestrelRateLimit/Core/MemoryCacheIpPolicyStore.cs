using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace KestrelRateLimit
{
    public class MemoryCacheIpPolicyStore : IIpPolicyStore
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheIpPolicyStore(IMemoryCache memoryCache, 
            IOptions<IpRateLimitOptions> options = null, 
            IOptions<IpRateLimitPolicies> policies = null)
        {
            _memoryCache = memoryCache;

            //save client rules defined in appsettings in cache on startup
            if(options != null && options.Value != null && policies != null && policies.Value != null && policies.Value.IpRules != null)
            {
                foreach (var rule in policies.Value.IpRules)
                {
                    Set($"{options.Value.IpPolicyPrefix}_{rule.Ip}", new IpRateLimitPolicy { Ip = rule.Ip, Rules = rule.Rules });
                }
            }
        }

        public void Set(string id, IpRateLimitPolicy policy)
        {
            _memoryCache.Set(id, policy);
        }

        public bool Exists(string id)
        {
            IpRateLimitPolicy stored;
            return _memoryCache.TryGetValue(id, out stored);
        }

        public IpRateLimitPolicy Get(string id)
        {
            IpRateLimitPolicy stored;
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
