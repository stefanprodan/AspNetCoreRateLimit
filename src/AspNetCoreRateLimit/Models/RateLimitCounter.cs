using System;

namespace AspNetCoreRateLimit
{
    /// <summary>
    /// Stores the initial access time and the numbers of calls made from that point
    /// </summary>
    public struct RateLimitCounter
    {
        public DateTime Timestamp { get; set; }

        public long TotalRequests { get; set; }

        //MP total request count
        public long TotalMPRequests { get; set; }
    }
}