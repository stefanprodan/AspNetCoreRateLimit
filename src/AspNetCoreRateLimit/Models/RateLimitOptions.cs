using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AspNetCoreRateLimit
{
    public class RateLimitOptions
    {
        public List<RateLimitRule> GeneralRules { get; set; }

        public List<string> EndpointWhitelist { get; set; }

        public List<string> ClientWhitelist { get; set; }

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

        /// <summary>
        /// Gets or sets a model that represents the QuotaExceeded response (content-type, content, status code).
        /// </summary>
        public QuotaExceededResponse QuotaExceededResponse { get; set; }

        /// <summary>
        /// Gets or sets the counter prefix, used to compose the rate limit counter cache key
        /// </summary>
        public string RateLimitCounterPrefix { get; set; } = "crlc";

        /// <summary>
        /// Gets or sets a value indicating whether all requests, including the rejected ones, should be stacked in this order: day, hour, min, sec
        /// </summary>
        public bool StackBlockedRequests { get; set; }

        /// <summary>
        /// Enables endpoint rate limiting based URL path and HTTP verb
        /// </summary>
        public bool EnableEndpointRateLimiting { get; set; }

        /// <summary>
        /// Disables X-Rate-Limit and Rety-After headers
        /// </summary>
        public bool DisableRateLimitHeaders { get; set; }

        /// <summary>
        /// Enabled the comparison logic to use Regex instead of wildcards.
        /// </summary>
        public bool EnableRegexRuleMatching { get; set; }

        /// <summary>
        /// Gets or sets behavior after the request is blocked
        /// </summary>
        public Func<HttpContext, ClientRequestIdentity, RateLimitCounter, RateLimitRule, Task> RequestBlockedBehavior { get; set; }
    }
}