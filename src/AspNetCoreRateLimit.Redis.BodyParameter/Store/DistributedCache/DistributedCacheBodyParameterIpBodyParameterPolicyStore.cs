using AspNetCoreRateLimit.Redis.BodyParameter.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace AspNetCoreRateLimit.Redis.BodyParameter.Store.DistributedCache
{
    public class DistributedCacheBodyParameterIpBodyParameterPolicyStore : DistributedCacheBodyParameterRateLimitStore<IpBodyParameterRateLimitPolicies>, IIpBodyParameterPolicyStore
    {
        private readonly IpBodyParameterRateLimitOptions _options;
        private readonly IpBodyParameterRateLimitPolicies _policies;

        public DistributedCacheBodyParameterIpBodyParameterPolicyStore(
            IDistributedCache cache,
            IOptions<IpBodyParameterRateLimitOptions> options = null,
            IOptions<IpBodyParameterRateLimitPolicies> policies = null) : base(cache)
        {
            _options = options?.Value;
            _policies = policies?.Value;
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