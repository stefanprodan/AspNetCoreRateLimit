using System;

namespace AspNetCoreRateLimit
{
    public class RateLimitRule
    {
        /// <summary>
        /// HTTP verb and path 
        /// </summary>
        /// <example>
        /// get:/api/values
        /// *:/api/values
        /// *
        /// </example>
        public string Endpoint { get; set; }

        /// <summary>
        /// A pattern used to identify the uniqueness of an endpoint
        /// </summary>
        public EPConterKeyMode ConterKeyMode { get; set; } = EPConterKeyMode.VerbAndPath;

        /// <summary>
        /// Rate limit period as in 1s, 1m, 1h
        /// </summary>
        public string Period { get; set; }

        public TimeSpan? PeriodTimespan { get; set; }

        /// <summary>
        /// Maximum number of requests that a client can make in a defined period
        /// </summary>
        public double Limit { get; set; }

        /// <summary>
        /// Gets or sets a model that represents the QuotaExceeded response (content-type, content, status code).
        /// </summary>
        public QuotaExceededResponse QuotaExceededResponse { get; set; }

        /// <summary>
        /// If MonitorMode is true requests that exceed the limit are only logged, and will execute successfully.
        /// </summary>
        public bool MonitorMode { get; set; } = false;
    }

    /// <summary>
    /// A pattern used to identify the uniqueness of an endpoint.
    /// </summary>
    public enum EPConterKeyMode
    {
        /// <summary>
        /// rate limit is applied to the combination of HTTP verb and path. <br/>
        /// if endpoint is *:/api/values. <br/>
        /// e.g. GET:/api/values/1 and POST:/api/values/1, GET:/api/values/2 and POST:/api/values/2 will be counted separately.
        /// </summary>
        VerbAndPath,
        /// <summary>
        /// Path - rate limit is applied to the combination of  path. <br/>
        /// if endpoint is *:/api/values. <br/>
        /// GET:/api/values and POST:/api/values will be counted together.
        /// </summary>
        Path,
        /// <summary>
        /// rate limit is applied to the combination of HTTP verb and rule. <br/>
        /// if endpoint is *:/api/values. <br/>
        ///GET:/api/values/1 POST:/api/values/2 will be counted separately. but GET:/api/values/1 and GET:/api/values/2 will be counted together.
        /// </summary>
        VerbAndRule,
        /// <summary>
        /// rate limit is applied to the combination of rule.<br/>
        /// if endpoint is *:/api/values. <br/>
        /// GET:/api/values/1 , POST:/api/values/1, GET:/api/values/2 and POST:/api/values/2 will be counted together.
        /// </summary>
        Rule
    }
}