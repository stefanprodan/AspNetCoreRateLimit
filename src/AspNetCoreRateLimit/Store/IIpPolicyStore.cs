using System.Threading.Tasks;

namespace AspNetCoreRateLimit
{
    public interface IIpPolicyStore : IRateLimitStore<IpRateLimitPolicies>
    {
        Task SeedAsync();
    }
}