using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace AspNetCoreRateLimit
{
    public class MemoryCacheIpPolicyStore : IIpPolicyStore
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheIpPolicyStore(IMemoryCache memoryCache, 
            IOptions<IpRateLimitOptions> options = null, 
            IOptions<IpRateLimitPolicies> policies = null)
        {
            _memoryCache = memoryCache;

            //save ip rules defined in appsettings in cache on startup
            if (options != null && options.Value != null && policies != null && policies.Value != null && policies.Value.IpRules != null)
            {
                Set($"{options.Value.IpPolicyPrefix}", policies.Value);

            }
            else // If set of rules is missing from appsettings
            {
                IpRateLimitPolicies defaultPolicies = new IpRateLimitPolicies
                {
                    IpRules = new List<IpRateLimitPolicy>
                    {
                        new IpRateLimitPolicy{
                            Ip = "127.0.0.2",
                            Rules = new List<RateLimitRule>(new RateLimitRule[] {
                                    new RateLimitRule {
                                        Endpoint = "*",
                                        Limit = 0,
                                        Period = "100y" }
                                })
                        }
                    }
                };

                Set($"{options.Value.IpPolicyPrefix}", defaultPolicies);
            }
        }

        public void Set(string id, IpRateLimitPolicies policy)
        {
            _memoryCache.Set(id, policy);
        }

        public bool Exists(string id)
        {
            IpRateLimitPolicies stored;
            return _memoryCache.TryGetValue(id, out stored);
        }

        public IpRateLimitPolicies Get(string id)
        {
            IpRateLimitPolicies stored;
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
