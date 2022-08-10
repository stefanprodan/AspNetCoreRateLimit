namespace AspNetCoreRateLimit.Redis.BodyParameter.Models
{
    public class ClientBodyParameterRateLimitPolicies
    {
        public List<ClientBodyParameterRateLimitPolicy> ClientRules { get; set; }
        
        public static string GetConfigurationName()
        {
            return nameof(ClientRateLimitPolicies);
        }
    }
}