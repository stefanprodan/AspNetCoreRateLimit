using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace AspNetCoreRateLimit.Redis.BodyParameter.Store.DistributedCache
{
    public class DistributedCacheBodyParameterRateLimitStore<T> : IBodyParameterRateLimitStore<T>
    {
        private readonly IDistributedCache _cache;

        public DistributedCacheBodyParameterRateLimitStore(IDistributedCache cache)
        {
            _cache = cache;
        }

        public void Set(string id, T entry, TimeSpan? expirationTime = null)
        {
            var options = new DistributedCacheEntryOptions();

            if (expirationTime.HasValue)
            {
                options.SetAbsoluteExpiration(expirationTime.Value);
            }

            _cache.SetString(id, JsonConvert.SerializeObject(entry), options);
        }

        public Task SetAsync(string id, T entry, TimeSpan? expirationTime = null, CancellationToken cancellationToken = default)
        {
            var options = new DistributedCacheEntryOptions();

            if (expirationTime.HasValue)
            {
                options.SetAbsoluteExpiration(expirationTime.Value);
            }

            return _cache.SetStringAsync(id, JsonConvert.SerializeObject(entry), options, cancellationToken);
        }

        public bool Exists(string id)
        {
            var stored = _cache.GetString(id);

            return !string.IsNullOrEmpty(stored);
        }

        public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
        {
            var stored = await _cache.GetStringAsync(id, cancellationToken);

            return !string.IsNullOrEmpty(stored);
        }

        public T Get(string id)
        {
            var stored = _cache.GetString(id);

            if (!string.IsNullOrEmpty(stored))
            {
                return JsonConvert.DeserializeObject<T>(stored);
            }

            return default;
        }

        public async Task<T> GetAsync(string id, CancellationToken cancellationToken = default)
        {
            var stored = await _cache.GetStringAsync(id, cancellationToken);

            if (!string.IsNullOrEmpty(stored))
            {
                return JsonConvert.DeserializeObject<T>(stored);
            }

            return default;
        }

        public void Remove(string id)
        { 
            _cache.Remove(id);
        }

        public Task RemoveAsync(string id, CancellationToken cancellationToken = default)
        {
            return _cache.RemoveAsync(id, cancellationToken);
        }
    }
}