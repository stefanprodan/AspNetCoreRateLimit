using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit
{
    public class ClientRateLimitProcessor : RateLimitProcessor, IRateLimitProcessor
    {
        private readonly ClientRateLimitOptions _options;
        private readonly IRateLimitStore<ClientRateLimitPolicy> _policyStore;

        public ClientRateLimitProcessor(
           ClientRateLimitOptions options,
           IRateLimitCounterStore counterStore,
           IClientPolicyStore policyStore,
           IRateLimitConfiguration config)
        : base(options, counterStore, new ClientCounterKeyBuilder(options), config)
        {
            _options = options;
            _policyStore = policyStore;
        }

        public async Task<IEnumerable<RateLimitRule>> GetMatchingRulesAsync(ClientRequestIdentity identity, CancellationToken cancellationToken = default)
        {
            var policy = await _policyStore.GetAsync($"{_options.ClientPolicyPrefix}_{identity.ClientId}", cancellationToken);

            return GetMatchingRules(identity, policy?.Rules);
        }
    }
}