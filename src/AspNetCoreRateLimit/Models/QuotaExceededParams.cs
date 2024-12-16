namespace AspNetCoreRateLimit.Models
{
    /// <summary>
    /// The parameters for the Quota Exceeded response
    /// </summary>
    public class QuotaExceededParams
    {
        /// <summary>
        /// The endpoint that was hit
        /// </summary>
        public string EndPoint { get; set; }

        /// <summary>
        /// quota exceeded path
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The max limit that was set
        /// </summary>
        public double Limit { get; set; }

        /// <summary>
        /// The period that the limit was set for
        /// </summary>
        public string Period { get; set; }

        /// <summary>
        /// The retry after time
        /// </summary>
        public string RetryAfter { get; set; }
    }
}
