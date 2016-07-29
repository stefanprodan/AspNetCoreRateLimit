using System.Collections.Generic;

namespace AspNetCoreRateLimit
{
    public class IpRateLimitOptions : RateLimitCoreOptions
    {
        /// <summary>
        /// Gets or sets the HTTP header of the real ip header injected by reverse proxy, by default is X-Real-IP
        /// </summary>
        public string RealIpHeader { get; set; } = "X-Real-IP";

        /// <summary>
        /// Gets or sets the HTTP header that holds the client identifier, by default is X-ClientId
        /// </summary>
        public string ClientIdHeader { get; set; } = "X-ClientId";

        /// <summary>
        /// Gets or sets the policy prefix, used to compose the client policy cache key
        /// </summary>
        public string IpPolicyPrefix { get; set; } = "ippp";

        public List<string> IpWhitelist { get; set; }
    }
}
