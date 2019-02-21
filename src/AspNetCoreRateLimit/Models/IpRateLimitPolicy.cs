namespace AspNetCoreRateLimit
{
    public class IpRateLimitPolicy : RateLimitPolicy
    {
        public string Ip { get; set; }
    }
}