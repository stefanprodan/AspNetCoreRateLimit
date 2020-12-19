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
        private readonly ICounterKeyBuilder _counterKeyBuilder;

        public ClientRateLimitProcessor(
                ClientRateLimitOptions options,
                IRateLimitCounterStore counterStore,
                IClientPolicyStore policyStore,
                IRateLimitConfiguration config,
                IProcessingStrategy processingStrategy)
            : base(options)
        {
            _options = options;
            _policyStore = policyStore;
            _counterKeyBuilder = new ClientCounterKeyBuilder(options);
            _processingStrategy = processingStrategy;
        }

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