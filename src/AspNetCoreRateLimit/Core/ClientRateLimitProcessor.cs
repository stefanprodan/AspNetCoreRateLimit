using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit
{
    public class ClientRateLimitProcessor : RateLimitProcessor, IRateLimitProcessor
    {
        private readonly ClientRateLimitOptions _options;
        private readonly IProcessingStrategy _processingStrategy;
        private readonly IRateLimitStore<ClientRateLimitPolicy> _policyStore;

        public ClientRateLimitProcessor(
                IProcessingStrategyFactory processingStrategyFactory,
                ClientRateLimitOptions options,
                IRateLimitCounterStore counterStore,
                IClientPolicyStore policyStore,
                IRateLimitConfiguration config)
            : base(options)
        {
            _options = options;
            _policyStore = policyStore;
            _processingStrategy = processingStrategyFactory.CreateProcessingStrategy(counterStore, new ClientCounterKeyBuilder(options), config, options);
        }

        public async Task<IEnumerable<RateLimitRule>> GetMatchingRulesAsync(ClientRequestIdentity identity, CancellationToken cancellationToken = default)
        {
            var policy = await _policyStore.GetAsync($"{_options.ClientPolicyPrefix}_{identity.ClientId}", cancellationToken);

            return GetMatchingRules(identity, policy?.Rules);
        }

        public async Task<RateLimitCounter> ProcessRequestAsync(ClientRequestIdentity requestIdentity, RateLimitRule rule, CancellationToken cancellationToken = default)
        {
            return await _processingStrategy.ProcessRequestAsync(requestIdentity, rule, cancellationToken);
        }
    }
}