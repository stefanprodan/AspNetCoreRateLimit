using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KestrelRateLimit
{
    public class RateLimitOptions
    {
        /// <summary>
        /// Gets or sets the global rate limit per second, set 0 to disable it
        /// </summary>
        public long PerSecond { get; set; }

        /// <summary>
        /// Gets or sets the global rate limit per minute, set 0 to disable it
        /// </summary>
        public long PerMinute { get; set; }

        /// <summary>
        /// Gets or sets the global rate limit per hour, set 0 to disable it
        /// </summary>
        public long PerHour { get; set; }

        /// <summary>
        /// Gets or sets the global rate limit per day, set 0 to disable it
        /// </summary>
        public long PerDay { get; set; }

        /// <summary>
        /// Gets or sets the global rate limit per week, set 0 to disable it
        /// </summary>
        public long PerWeek { get; set; }

        /// <summary>
        /// Enables IP rate limiting
        /// </summary>
        public bool EnableIpRateLimiting { get; set; } = true;

        /// <summary>
        /// Enables client rate limiting based on ClientIdHeader
        /// </summary>
        public bool EnableClientRateLimiting { get; set; }

        /// <summary>
        /// Enables endpoint rate limiting based URL path and HTTP verb
        /// </summary>
        public bool EnableEndpointRateLimiting { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether all requests, including the rejected ones, should be stacked in this order: day, hour, min, sec
        /// </summary>
        public bool StackBlockedRequests { get; set; }

        /// <summary>
        /// Gets or sets the HTTP header of the client unique identifier, by default is X-ClientId
        /// </summary>
        public string ClientIdHeader { get; set; } = "X-ClientId";

        /// <summary>
        /// Gets or sets the HTTP Status code returned when rate limiting occurs, by default value is set to 429 (Too Many Requests)
        /// </summary>
        public int HttpStatusCode { get; set; } = 429;

        /// <summary>
        /// Gets or sets a value that will be used as a formatter for the QuotaExceeded response message.
        /// If none specified the default will be: 
        /// API calls quota exceeded! maximum admitted {0} per {1}
        /// </summary>
        public string QuotaExceededMessage { get; set; }

        public List<string> IpWhitelist { get; set; }

        public List<RateLimits>  IpRules { get; set; } = new List<RateLimits>();

        public List<string> EndpointWhitelist { get; set; }

        public List<RateLimits> EndpointRules { get; set; } = new List<RateLimits>();

        public List<string> ClientWhitelist { get; set; }

        public List<RateLimits> ClientRules { get; set; } = new List<RateLimits>();

        public bool StoreOptionsInCache { get; set; }

        /// <summary>
        /// Gets or sets the app name, used to compose the cache key
        /// </summary>
        public string ApplicationName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the counter prefix, used to compose the cache key
        /// </summary>
        public string RateLimitCounterPrefix { get; set; } = "ratelimit_counter";

        /// <summary>
        /// Gets or sets the options prefix, used to compose the cache key
        /// </summary>
        public string RateLimitOptionsPrefix { get; set; } = "ratelimit_options";

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
        /// Returns the options key (global prefix + policy key suffix)
        /// </summary>
        /// <returns>
        /// The policy key.
        /// </returns>
        public string GetOptionsKey()
        {
            return ApplicationName + RateLimitOptionsPrefix;
        }



        public Dictionary<RateLimitPeriod, long> ComputeRates()
        {
            var rates = new Dictionary<RateLimitPeriod, long>();

            rates.Add(RateLimitPeriod.Second, PerSecond);
            rates.Add(RateLimitPeriod.Minute, PerMinute);
            rates.Add(RateLimitPeriod.Hour, PerHour);
            rates.Add(RateLimitPeriod.Day, PerDay);
            rates.Add(RateLimitPeriod.Week, PerWeek);

            return rates;
        }

    }
}
