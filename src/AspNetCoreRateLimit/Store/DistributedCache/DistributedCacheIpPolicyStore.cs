using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace AspNetCoreRateLimit
{
    public class DistributedCacheIpPolicyStore(
        IDistributedCache cache,
        IOptions<IpRateLimitOptions> options = null,
        IOptions<IpRateLimitPolicies> policies = null) : DistributedCacheRateLimitStore<IpRateLimitPolicies>(cache), IIpPolicyStore
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