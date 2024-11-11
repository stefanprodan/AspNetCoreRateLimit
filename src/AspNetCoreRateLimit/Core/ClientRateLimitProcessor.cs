using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit
{
    public class ClientRateLimitProcessor(
            ClientRateLimitOptions options,
            IClientPolicyStore policyStore,
            IProcessingStrategy processingStrategy) : RateLimitProcessor(options), IRateLimitProcessor
    {
        private readonly ClientRateLimitOptions _options = options;
        private readonly IProcessingStrategy _processingStrategy = processingStrategy;
        private readonly IRateLimitStore<ClientRateLimitPolicy> _policyStore = policyStore;
        private readonly ICounterKeyBuilder _counterKeyBuilder = new ClientCounterKeyBuilder(options);

        public async Task<IEnumerable<RateLimitRule>> GetMatchingRulesAsync(ClientRequestIdentity identity, CancellationToken cancellationToken = default)
        {
            var policy = await _policyStore.GetAsync($"{_options.ClientPolicyPrefix}_{identity.ClientId}", cancellationToken);

            return GetMatchingRules(identity, policy?.Rules);
        }

        public async Task<RateLimitCounter> ProcessRequestAsync(ClientRequestIdentity requestIdentity, RateLimitRule rule, CancellationToken cancellationToken = default)
        {
            return await _processingStrategy.ProcessRequestAsync(requestIdentity, rule, _counterKeyBuilder, _options, cancellationToken);
        }
    }
}