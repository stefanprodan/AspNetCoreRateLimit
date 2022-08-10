namespace AspNetCoreRateLimit.Redis.BodyParameter.Models
{
    public class IpBodyParameterRateLimitPolicy : BodyParameterRateLimitPolicy
    {
        public string Ip { get; set; }
    }
}