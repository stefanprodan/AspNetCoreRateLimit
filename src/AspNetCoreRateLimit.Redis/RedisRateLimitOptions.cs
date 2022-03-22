namespace AspNetCoreRateLimit.Redis
{
    public class RedisRateLimitOptions
    {
        /// <summary>
        ///     Gets or sets the Redis key prefix
        /// </summary>
        public string KeyPrefix { get; set; } = "ratelimits:";
    }
}