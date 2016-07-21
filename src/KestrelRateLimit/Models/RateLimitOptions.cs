using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KestrelRateLimit
{
    public class RateLimitOptions
    {
        public string ApplicationName { get; set; } = string.Empty;

        public string RateLimitCounterPrefix { get; set; } = "counter";

        public string RateLimitPolicyPrefix { get; set; } = "policy";

        /// <summary>
        /// Returns key prefix for rate limits
        /// </summary>
        /// <returns>
        /// The counter key.
        /// </returns>
        public string GetCounterKey()
        {
            return ApplicationName + RateLimitCounterPrefix;
        }

        /// <summary>
        /// Returns the policy key (global prefix + policy key suffix)
        /// </summary>
        /// <returns>
        /// The policy key.
        /// </returns>
        public string GetPolicyKey()
        {
            return ApplicationName + RateLimitPolicyPrefix;
        }

        public long PerSecond { get; set; }

        public long PerMinute { get; set; }

        public long PerHour { get; set; }

        public long PerDay { get; set; }

        public long PerWeek { get; set; }

        public bool EnableIpRateLimiting { get; set; } = true;

        public bool EnableClientRateLimiting { get; set; }

        public bool EnableEndpointRateLimiting { get; set; }

        public bool StackBlockedRequests { get; set; }

        public string ClientIdHeader { get; set; } = "X-ClientId";

        public int HttpStatusCode { get; set; } = 429;

        public List<string> IpWhitelist { get; set; }

        public List<RateLimits>  IpRules { get; set; } = new List<RateLimits>();

        public List<string> EndpointWhitelist { get; set; }

        public List<RateLimits> EndpointRules { get; set; } = new List<RateLimits>();

        public List<string> ClientWhitelist { get; set; }

        public List<RateLimits> ClientRules { get; set; } = new List<RateLimits>();


    }
}
