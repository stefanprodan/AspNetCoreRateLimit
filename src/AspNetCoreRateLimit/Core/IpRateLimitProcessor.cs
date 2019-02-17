using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit
{
    public class IpRateLimitProcessor : RateLimitProcessor, IRateLimitProcessor
    {
        private readonly IpRateLimitOptions _options;
        private readonly IRateLimitStore<IpRateLimitPolicies> _policyStore;

        public IpRateLimitProcessor(
           IpRateLimitOptions options,
           IRateLimitCounterStore counterStore,
           IIpPolicyStore policyStore)
        : base(options, counterStore)
        {
            _options = options;
            _policyStore = policyStore;
        }

        public async Task<IEnumerable<RateLimitRule>> GetMatchingRulesAsync(ClientRequestIdentity identity, CancellationToken cancellationToken = default)
        {
            var limits = new List<RateLimitRule>();
            var policies = await _policyStore.GetAsync($"{_options.IpPolicyPrefix}", cancellationToken);

            if (policies != null && policies.IpRules != null && policies.IpRules.Any())
            {
                // search for rules with IP intervals containing client IP
                var matchPolicies = policies.IpRules.Where(r => IpParser.ContainsIp(r.Ip, identity.ClientIp)).AsEnumerable();
                var rules = new List<RateLimitRule>();

                foreach (var item in matchPolicies)
                {
                    rules.AddRange(item.Rules);
                }

                return GetMatchingRules(identity, rules);
            }

            return Enumerable.Empty<RateLimitRule>();
        }

        public override bool IsWhitelisted(ClientRequestIdentity requestIdentity)
        {
            if (_options.IpWhitelist != null && IpParser.ContainsIp(_options.IpWhitelist, requestIdentity.ClientIp))
            {
                return true;
            }

            return base.IsWhitelisted(requestIdentity);
        }

        protected override string GetCounterKey(ClientRequestIdentity requestIdentity, RateLimitRule rule)
        {
            return $"{_options.RateLimitCounterPrefix}_{requestIdentity.ClientIp}_{rule.Period}";
        }
    }
}