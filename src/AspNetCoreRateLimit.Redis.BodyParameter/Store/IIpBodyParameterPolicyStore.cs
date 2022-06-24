using AspNetCoreRateLimit.Redis.BodyParameter.Models;

namespace AspNetCoreRateLimit.Redis.BodyParameter.Store
{
    public interface IIpBodyParameterPolicyStore : IBodyParameterRateLimitStore<IpBodyParameterRateLimitPolicies>
    {
        Task SeedAsync();
    }
}