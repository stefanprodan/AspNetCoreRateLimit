using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AspNetCoreRateLimit
{
    public class DistributedCacheIpPolicyStore : IIpPolicyStore
    {
        private readonly IDistributedCache _memoryCache;

        public DistributedCacheIpPolicyStore(
            IDistributedCache memoryCache, 
            IOptions<IpRateLimitOptions> options = null, 
            IOptions<IpRateLimitPolicies> policies = null)
        {
            _memoryCache = memoryCache;

            var ipOptions = options?.Value;
            var ipPolicyRules = policies?.Value;

            //save IP rules defined in appsettings in cache on startup
            if (ipOptions != null && ipPolicyRules != null)
            {
                Set($"{ipOptions.IpPolicyPrefix}", ipPolicyRules);

            }
        }

        public void Set(string id, IpRateLimitPolicies policy)
        {
            _memoryCache.SetString(id, JsonConvert.SerializeObject(policy));
        }

        public bool Exists(string id)
        {
            var stored = _memoryCache.GetString(id);
            return !string.IsNullOrEmpty(stored);
        }

        public IpRateLimitPolicies Get(string id)
        {
            var stored = _memoryCache.GetString(id);
            if (!string.IsNullOrEmpty(stored))
            {
                return JsonConvert.DeserializeObject<IpRateLimitPolicies>(stored);
            }
            return null;
        }

        public void Remove(string id)
        {
            _memoryCache.Remove(id);
        }
    }
}
