using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace AspNetCoreRateLimit
{
    public class MPRateLimitProcessor: RateLimitProcessor , IRateLimitProcessor
    {
        private readonly MPRateLimitOptions _options;
        private readonly IRateLimitStore<MPRateLimitPolicy> _policyStore;

        public MPRateLimitProcessor(
          MPRateLimitOptions options,
          IRateLimitCounterStore counterStore,
          IMPPolicyStore policyStore,
          IRateLimitConfiguration config)
       : base(options, counterStore, new MPRateCounterKeyBuilder(options), config)
        {
            _options = options;
            _policyStore = policyStore;  
        }

        //get matching rule for MP rate limit
        public async Task<IEnumerable<RateLimitRule>> GetMatchingRulesAsync(ClientRequestIdentity identity, CancellationToken cancellationToken = default)
        {
            var policy = await _policyStore.GetAsync($"{_options.MPRatePolicyPrefix}_{identity.ClientId}", cancellationToken);
            return GetMatchingRules(identity, policy?.Rules);
        }

    }
}
