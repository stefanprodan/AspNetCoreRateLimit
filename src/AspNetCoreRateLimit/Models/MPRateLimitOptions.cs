namespace AspNetCoreRateLimit
{
    public class MPRateLimitOptions : RateLimitOptions
    {
        /// <summary>
        /// Gets or sets the HTTP header that holds the MPData, by default is X-UIPATH-Metadata
        /// </summary>
        public string MPRateHeader { get; set; } = "X-UIPATH-Metadata";

        /// <summary>
        /// Gets or sets the policy prefix, used to compose the client policy cache key
        /// </summary>
        public string MPRatePolicyPrefix { get; set; } = "mplp";
    }
}
