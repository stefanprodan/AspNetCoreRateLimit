using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KestrelRateLimit
{
    public class IpRateLimitPolicy
    {
        public string Ip { get; set; }
        public List<RateLimitRule> Rules { get; set; }
    }
}
