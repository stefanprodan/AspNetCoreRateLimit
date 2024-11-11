using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace AspNetCoreRateLimit
{
    public class MemoryCacheIpPolicyStore(
        IMemoryCache cache,
        IOptions<IpRateLimitOptions> options = null,
        IOptions<IpRateLimitPolicies> policies = null) : MemoryCacheRateLimitStore<IpRateLimitPolicies>(cache), IIpPolicyStore
    {
        private readonly IpRateLimitOptions _options = options?.Value;
        private readonly IpRateLimitPolicies _policies = policies?.Value;

        public async Task SeedAsync()
        {
            // on startup, save the IP rules defined in appsettings
            if (_options != null && _policies != null)
            {
                await SetAsync($"{_options.IpPolicyPrefix}", _policies).ConfigureAwait(false);
            }
        }
    }
}