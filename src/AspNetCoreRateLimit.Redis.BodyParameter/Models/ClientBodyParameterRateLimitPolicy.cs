namespace AspNetCoreRateLimit.Redis.BodyParameter.Models
{
    public class ClientBodyParameterRateLimitPolicy : BodyParameterRateLimitPolicy
    {
        public string ClientId { get; set; }
    }
}