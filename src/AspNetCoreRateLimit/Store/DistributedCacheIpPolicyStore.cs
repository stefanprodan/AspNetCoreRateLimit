using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AspNetCoreRateLimit
{
    public class DistributedCacheIpPolicyStore : IIpPolicyStore
    {
        private readonly IDistributedCache _memoryCache;

        public DistributedCacheIpPolicyStore(IDistributedCache memoryCache, 
            IOptions<IpRateLimitOptions> options = null, 
            IOptions<IpRateLimitPolicies> policies = null)
        {
            _memoryCache = memoryCache;

            //save ip rules defined in appsettings in distributed cache on startup
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
            _memoryCache.SetString(id, JsonConvert.SerializeObject(policy));
        }

        public bool Exists(string id)
        {
            var stored = _memoryCache.GetString(id);
            return !string.IsNullOrEmpty(stored);
        }

        public IpRateLimitPolicies Get(string id)
        {
            var stored = _memoryCache.GetString(id);
            if (!string.IsNullOrEmpty(stored))
            {
                return JsonConvert.DeserializeObject<IpRateLimitPolicies>(stored);
            }
            return null;
        }

        public void Remove(string id)
        {
            _memoryCache.Remove(id);
        }
    }
}
