namespace AspNetCoreRateLimit.Redis.BodyParameter.Models
{
    public class IpBodyParameterRateLimitPolicies
    {
        public List<IpBodyParameterRateLimitPolicy> IpRules { get; set; } = new List<IpBodyParameterRateLimitPolicy>();
    }
}