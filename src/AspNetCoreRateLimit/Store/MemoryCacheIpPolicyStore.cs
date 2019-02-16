using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace AspNetCoreRateLimit
{
    public class MemoryCacheIpPolicyStore : IIpPolicyStore
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheIpPolicyStore(
            IMemoryCache memoryCache, 
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
            _memoryCache.Set(id, policy);
        }

        public bool Exists(string id)
        {
            return _memoryCache.TryGetValue(id, out _);
        }

        public IpRateLimitPolicies Get(string id)
        {
            if (_memoryCache.TryGetValue(id, out IpRateLimitPolicies stored))
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