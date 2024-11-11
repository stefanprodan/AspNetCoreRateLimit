using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit
{
    public class IpRateLimitProcessor(
            IpRateLimitOptions options,
            IIpPolicyStore policyStore,
            IProcessingStrategy processingStrategy) : RateLimitProcessor(options), IRateLimitProcessor
    {
        private readonly IpRateLimitOptions _options = options;
        private readonly IRateLimitStore<IpRateLimitPolicies> _policyStore = policyStore;
        private readonly IProcessingStrategy _processingStrategy = processingStrategy;
        private readonly ICounterKeyBuilder _counterKeyBuilder = new IpCounterKeyBuilder(options);

        public async Task<IEnumerable<RateLimitRule>> GetMatchingRulesAsync(ClientRequestIdentity identity, CancellationToken cancellationToken = default)
        {
            var policies = await _policyStore.GetAsync($"{_options.IpPolicyPrefix}", cancellationToken);

            var rules = new List<RateLimitRule>();

            if (policies?.IpRules?.Any() == true)
            {
                // search for rules with IP intervals containing client IP
                var matchPolicies = policies.IpRules.Where(r => IpParser.ContainsIp(r.Ip, identity.ClientIp));

                foreach (var item in matchPolicies)
                {
                    rules.AddRange(item.Rules);
                }
            }

            return GetMatchingRules(identity, rules);
        }

        public async Task<RateLimitCounter> ProcessRequestAsync(ClientRequestIdentity requestIdentity, RateLimitRule rule, CancellationToken cancellationToken = default)
        {
            return await _processingStrategy.ProcessRequestAsync(requestIdentity, rule, _counterKeyBuilder, _options, cancellationToken);
        }
    }
}