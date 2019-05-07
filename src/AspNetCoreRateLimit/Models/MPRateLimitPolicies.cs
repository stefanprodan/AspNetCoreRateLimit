using System.Collections.Generic;

namespace AspNetCoreRateLimit
{
    public class MPRateLimitPolicies
    {
        public List<MPRateLimitPolicy> MPRules { get; set; }
    }
}
