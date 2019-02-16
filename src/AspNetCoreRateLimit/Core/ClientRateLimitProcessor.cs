using System.Collections.Generic;
using System.Linq;

namespace AspNetCoreRateLimit
{
    public class ClientRateLimitProcessor : RateLimitProcessor, IRateLimitProcessor
    {
        private readonly ClientRateLimitOptions _options;
        private readonly IPolicyStore<ClientRateLimitPolicy> _policyStore;

        public ClientRateLimitProcessor(
           ClientRateLimitOptions options,
           IRateLimitCounterStore counterStore,
           IClientPolicyStore policyStore)
        : base(options, counterStore)
        {
            _options = options;
            _policyStore = policyStore;
        }

        public IEnumerable<RateLimitRule> GetMatchingRules(ClientRequestIdentity identity)
        {
            var policy = _policyStore.Get($"{_options.ClientPolicyPrefix}_{identity.ClientId}");

            if (policy != null)
            {
                return GetMatchingRules(identity, policy.Rules);
            }

            return Enumerable.Empty<RateLimitRule>();
        }

        protected override string GetCounterKey(ClientRequestIdentity requestIdentity, RateLimitRule rule)
        {
            return $"{_options.RateLimitCounterPrefix}_{requestIdentity.ClientId}_{rule.Period}";
        }
    }
}