using System.Collections.Generic;

namespace AspNetCoreRateLimit
{
    public class RateLimitPolicy
    {
        public List<RateLimitRule> Rules { get; set; } = new List<RateLimitRule>();
    }
}
