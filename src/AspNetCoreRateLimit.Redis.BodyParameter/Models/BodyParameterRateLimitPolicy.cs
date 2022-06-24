namespace AspNetCoreRateLimit.Redis.BodyParameter.Models
{
    public class BodyParameterRateLimitPolicy
    {
        public List<BodyParameterRateLimitRule> Rules { get; set; } = new List<BodyParameterRateLimitRule>();
    }
}
