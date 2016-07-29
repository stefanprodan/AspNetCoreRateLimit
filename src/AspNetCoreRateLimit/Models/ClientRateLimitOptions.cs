namespace AspNetCoreRateLimit
{
    public class ClientRateLimitOptions : RateLimitCoreOptions
    {
        /// <summary>
        /// Gets or sets the HTTP header that holds the client identifier, by default is X-ClientId
        /// </summary>
        public string ClientIdHeader { get; set; } = "X-ClientId";

        /// <summary>
        /// Gets or sets the policy prefix, used to compose the client policy cache key
        /// </summary>
        public string ClientPolicyPrefix { get; set; } = "crlp";
    }
}
