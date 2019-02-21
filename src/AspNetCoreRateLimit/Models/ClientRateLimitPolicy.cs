namespace AspNetCoreRateLimit
{
    public class ClientRateLimitPolicy : RateLimitPolicy
    {
        public string ClientId { get; set; }
    }
}