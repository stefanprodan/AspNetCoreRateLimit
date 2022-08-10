using AspNetCoreRateLimit.Redis.BodyParameter.Models;

namespace AspNetCoreRateLimit.Redis.BodyParameter.Store
{
    public interface IClientBodyParameterPolicyStore : IBodyParameterRateLimitStore<ClientBodyParameterRateLimitPolicy>
    {
        Task SeedAsync();
    }
}