using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KestrelRateLimit
{
    public class ClientRateLimitProcessor
    {
        private readonly ClientRateLimitOptions _options;
        private readonly IRateLimitCounterStore _counterStore;
        private readonly IClientPolicyStore _policyStore;

        private static readonly object _processLocker = new object();

        public ClientRateLimitProcessor(ClientRateLimitOptions options,
           IRateLimitCounterStore counterStore,
           IClientPolicyStore policyStore)
        {
            _options = options;
            _counterStore = counterStore;
            _policyStore = policyStore;
        }


        public List<ClientRateLimit> GetMatchingLimits(ClientRequestIdentity identity)
        {
            var limits = new List<ClientRateLimit>();
            var policy = _policyStore.Get($"{_options.ClientPolicyPrefix}_{identity.ClientId}");

            var globalLimit = policy.Limits.FirstOrDefault(l => l.Endpoint == "*");
            if(globalLimit != null)
            {
                limits.Add(globalLimit);
            }

            var pathLimits = policy.Limits.Where(l => $"*:{identity.Path}".Contains(l.Endpoint)).AsEnumerable();
            limits.AddRange(pathLimits);

            var verbLimits = policy.Limits.Where(l => $"{identity.HttpVerb}:{identity.Path}".Contains(l.Endpoint)).AsEnumerable();
            limits.AddRange(verbLimits);


            return limits;
        }
    }
}
