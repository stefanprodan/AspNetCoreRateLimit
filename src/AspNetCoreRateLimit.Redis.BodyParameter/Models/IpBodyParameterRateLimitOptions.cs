namespace AspNetCoreRateLimit.Redis.BodyParameter.Models
{
    public class IpBodyParameterRateLimitOptions : BodyParameterRateLimitOptions
    {
        /// <summary>
        /// Gets or sets the policy prefix, used to compose the client policy cache key
        /// </summary>
        public string IpPolicyPrefix { get; set; } = "ippp";
    }
}