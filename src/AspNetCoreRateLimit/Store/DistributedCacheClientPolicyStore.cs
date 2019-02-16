using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AspNetCoreRateLimit
{
    public class DistributedCacheClientPolicyStore : IClientPolicyStore
    {
        private readonly IDistributedCache _memoryCache;

        public DistributedCacheClientPolicyStore(
            IDistributedCache memoryCache, 
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
            _memoryCache.SetString(id, JsonConvert.SerializeObject(policy));
        }

        public bool Exists(string id)
        {
            var stored = _memoryCache.GetString(id);

            return !string.IsNullOrEmpty(stored);
        }

        public ClientRateLimitPolicy Get(string id)
        {
            var stored = _memoryCache.GetString(id);

            if (!string.IsNullOrEmpty(stored))
            {
                return JsonConvert.DeserializeObject<ClientRateLimitPolicy>(stored);
            }

            return null;
        }

        public void Remove(string id)
        {
            _memoryCache.Remove(id);
        }
    }
}