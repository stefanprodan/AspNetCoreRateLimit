using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace AspNetCoreRateLimit
{
    public class DistributedCacheClientPolicyStore : DistributedCacheRateLimitStore<ClientRateLimitPolicy>, IClientPolicyStore
    {
        private readonly ClientRateLimitOptions _options;
        private readonly ClientRateLimitPolicies _policies;

        public DistributedCacheClientPolicyStore(
            IDistributedCache cache,
            IOptions<ClientRateLimitOptions> options = null,
            IOptions<ClientRateLimitPolicies> policies = null) : base(cache)
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
                    await SetAsync($"{_options.ClientPolicyPrefix}_{rule.ClientId}", new ClientRateLimitPolicy { ClientId = rule.ClientId, Rules = rule.Rules }).ConfigureAwait(false);
                }
            }
        }
    }
}