using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace AspNetCoreRateLimit
{
    public class StackExchangeRedisIpPolicyStore : StackExchangeRedisRateLimitStore<IpRateLimitPolicies>, IIpPolicyStore
    {
        private readonly IpRateLimitOptions _options;
        private readonly IpRateLimitPolicies _policies;

        public StackExchangeRedisIpPolicyStore(
            IConnectionMultiplexer redis,
            IOptions<IpRateLimitOptions> options = null,
            IOptions<IpRateLimitPolicies> policies = null) : base(redis)
        {
            _options = options?.Value;
            _policies = policies?.Value;
        }

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