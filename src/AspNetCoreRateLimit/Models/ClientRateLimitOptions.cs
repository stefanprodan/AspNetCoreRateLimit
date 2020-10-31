namespace AspNetCoreRateLimit
{
    public class ClientRateLimitOptions : RateLimitOptions
    {
        /// <summary>
        /// Gets or sets the policy prefix, used to compose the client policy cache key
        /// </summary>
        public string ClientPolicyPrefix { get; set; } = "crlp";
    }
}