using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace AspNetCoreRateLimit
{
    public class MemoryCacheClientPolicyStore : IClientPolicyStore
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheClientPolicyStore(
            IMemoryCache memoryCache, 
            IOptions<ClientRateLimitOptions> options = null, 
            IOptions<ClientRateLimitPolicies> policies = null)
        {
            _memoryCache = memoryCache;

            var clientOptions = options?.Value;
            var clientPolicyRules = policies?.Value?.ClientRules;

            //save client rules defined in appsettings in cache on startup
            if (clientOptions != null && clientPolicyRules != null)
            {
                foreach (var rule in clientPolicyRules)
                {
                    Set($"{clientOptions.ClientPolicyPrefix}_{rule.ClientId}", new ClientRateLimitPolicy { ClientId = rule.ClientId, Rules = rule.Rules });
                }
            }
        }

        public void Set(string id, ClientRateLimitPolicy policy)
        {
            _memoryCache.Set(id, policy);
        }

        public bool Exists(string id)
        {
            return _memoryCache.TryGetValue(id, out _);
        }

        public ClientRateLimitPolicy Get(string id)
        {
            if (_memoryCache.TryGetValue(id, out ClientRateLimitPolicy stored))
            {
                return stored;
            }

            return null;
        }

        public void Remove(string id)
        {
            _memoryCache.Remove(id);
        }
    }
}