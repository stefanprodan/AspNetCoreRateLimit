using System.Collections.Generic;

namespace AspNetCoreRateLimit
{
    public class IpRateLimitOptions : RateLimitOptions
    {
        /// <summary>
        /// Gets or sets the HTTP header of the real ip header injected by reverse proxy, by default is X-Real-IP
        /// </summary>
        public string RealIpHeader { get; set; } = "X-Real-IP";

        /// <summary>
        /// Gets or sets the policy prefix, used to compose the client policy cache key
        /// </summary>
        public string IpPolicyPrefix { get; set; } = "ippp";
    }
}