using System.Collections.Generic;

namespace AspNetCoreRateLimit
{
    public class ClientRateLimitPolicies
    {
        public List<ClientRateLimitPolicy> ClientRules { get; set; }
    }
}