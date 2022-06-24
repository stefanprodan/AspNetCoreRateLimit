namespace AspNetCoreRateLimit.Redis.BodyParameter.Models
{
    public class BodyParameterRateLimitRule
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
        /// Rate limit period as in 1s, 1m, 1h
        /// </summary>
        public string Period { get; set; }
        
        public TimeSpan? PeriodTimespan => Period.ToTimeSpan();
        
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
        
        public bool EnableBodyParameter { get; set; }

        public List<EndpointBodyParameterRateLimitRule> BodyParameters { get; set; }
    }

    public class EndpointBodyParameterRateLimitRule
    {
        public string ParameterName { get; set; }
        
        public List<string> ParameterValues { get; set; }
        
        /// <summary>
        /// Rate limit period as in 1s, 1m, 1h
        /// </summary>
        public string Period { get; set; }

        public TimeSpan? PeriodTimespan => Period.ToTimeSpan();

        /// <summary>
        /// Maximum number of requests that a client can make in a defined period
        /// </summary>
        public double Limit { get; set; }
    }
}