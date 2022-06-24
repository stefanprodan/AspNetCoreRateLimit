using AspNetCoreRateLimit.Redis.BodyParameter.Models;

namespace AspNetCoreRateLimit.Redis.BodyParameter.Store
{
    public interface IBodyParameterRateLimitCounterStore : IBodyParameterRateLimitStore<BodyParameterRateLimitCounter?>
    {
    }
}