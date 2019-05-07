using System.Threading.Tasks;

namespace AspNetCoreRateLimit
{
    public interface IMPPolicyStore : IRateLimitStore<MPRateLimitPolicy>
    {
        Task SeedAsync();
    }
}