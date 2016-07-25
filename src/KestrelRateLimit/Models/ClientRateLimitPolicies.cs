using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KestrelRateLimit
{
    public class ClientRateLimitPolicies
    {
        public List<ClientRateLimitPolicy> Policies { get; set; }
    }
}
