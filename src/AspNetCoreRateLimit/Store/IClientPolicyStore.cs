using System.Threading.Tasks;

namespace AspNetCoreRateLimit
{
    public interface IClientPolicyStore : IRateLimitStore<ClientRateLimitPolicy>
    {
        Task SeedAsync();
    }
}