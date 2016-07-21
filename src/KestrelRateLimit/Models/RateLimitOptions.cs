using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KestrelRateLimit
{
    public class RateLimitOptions
    {
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
    }
}
