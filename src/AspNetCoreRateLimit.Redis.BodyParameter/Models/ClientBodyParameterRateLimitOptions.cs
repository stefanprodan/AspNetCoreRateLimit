namespace AspNetCoreRateLimit.Redis.BodyParameter.Models
{
    public class ClientBodyParameterRateLimitOptions : BodyParameterRateLimitOptions
    {
        /// <summary>
        /// Gets or sets the policy prefix, used to compose the client policy cache key
        /// </summary>
        public string ClientPolicyPrefix { get; set; } = "crlp";

        public static string GetConfigurationName()
        {
            return nameof(ClientRateLimitOptions);
        }
    }
}