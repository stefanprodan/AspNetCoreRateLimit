using AspNetCoreRateLimit.Redis.BodyParameter.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace AspNetCoreRateLimit.Redis.BodyParameter.Store.DistributedCache
{
    public class DistributedCacheBodyParameterClientBodyParameterPolicyStore : DistributedCacheBodyParameterRateLimitStore<ClientBodyParameterRateLimitPolicy>, IClientBodyParameterPolicyStore
    {
        private readonly ClientBodyParameterRateLimitOptions _options;
        private readonly ClientBodyParameterRateLimitPolicies _policies;

        public DistributedCacheBodyParameterClientBodyParameterPolicyStore(
            IDistributedCache cache,
            IOptions<ClientBodyParameterRateLimitOptions> options = null,
            IOptions<ClientBodyParameterRateLimitPolicies> policies = null) : base(cache)
        {
            _options = options?.Value;
            _policies = policies?.Value;
        }

        public async Task SeedAsync()
        {
            // on startup, save the IP rules defined in appsettings
            if (_options != null && _policies?.ClientRules != null)
            {
                foreach (var rule in _policies.ClientRules)
                {
                    await SetAsync($"{_options.ClientPolicyPrefix}_{rule.ClientId}",  new ClientBodyParameterRateLimitPolicy { ClientId = rule.ClientId, Rules = rule.Rules }).ConfigureAwait(false);
                }
            }
        }
    }
}