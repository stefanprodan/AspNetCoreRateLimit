using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AspNetCoreRateLimit
{
    public class DistributedCacheIpPolicyStore : DistributedCacheRateLimitStore<IpRateLimitPolicies>, IIpPolicyStore
    {
        private readonly IpRateLimitOptions _options;
        private readonly IpRateLimitPolicies _policies;

        public DistributedCacheIpPolicyStore(
            IDistributedCache cache,
            IOptions<IpRateLimitOptions> options = null,
            IOptions<IpRateLimitPolicies> policies = null) : base(cache)
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

        public async Task SeedAsync()
        {
            // on startup, save the IP rules defined in appsettings
            if (_options != null && _policies != null)
            {
                await SetAsync($"{_options.IpPolicyPrefix}", _policies).ConfigureAwait(false);
            }
        }
    }
}