using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace AspNetCoreRateLimit
{
    public class MemoryCacheMPPolicyStore : MemoryCacheRateLimitStore<MPRateLimitPolicy>, IMPPolicyStore
    {
        private readonly MPRateLimitOptions _options;
        private readonly MPRateLimitPolicies _policies;

        public MemoryCacheMPPolicyStore(
            IMemoryCache cache,
            IOptions<MPRateLimitOptions> options = null,
            IOptions<MPRateLimitPolicies> policies = null) : base(cache)
        {
            _options = options?.Value;
            _policies = policies?.Value;
        }

        public async Task SeedAsync()
        {
            // on startup, save the MP rules defined in appsettings
            if (_options != null && _policies != null)
            { 
                foreach (var rule in _policies.MPRules)
                {
                    await SetAsync($"{_options.MPRatePolicyPrefix}_{rule.MpMachineId}", new MPRateLimitPolicy { MpMachineId = rule.MpMachineId, Rules = rule.Rules }).ConfigureAwait(false);

                }

            }
        }
    }
}