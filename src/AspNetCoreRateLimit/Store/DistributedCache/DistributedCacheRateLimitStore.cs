using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit
{
    public class DistributedCacheRateLimitStore<T> : IRateLimitStore<T>
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<DistributedCacheRateLimitStore<T>> _logger;

        public DistributedCacheRateLimitStore(IDistributedCache cache, ILogger<DistributedCacheRateLimitStore<T>> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public Task SetAsync(string id, T entry, TimeSpan? expirationTime = null, CancellationToken cancellationToken = default)
        {
            var options = new DistributedCacheEntryOptions();

            if (expirationTime.HasValue)
            {
                options.SetAbsoluteExpiration(expirationTime.Value);
            }
            try
            {
                return _cache.SetStringAsync(id, JsonConvert.SerializeObject(entry), options, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error occurred: {ex.Message}");
                return Task.CompletedTask;
            }
        }

        public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                var stored = await _cache.GetStringAsync(id, cancellationToken);
                return !string.IsNullOrEmpty(stored);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error occurred: {ex.Message}");
                return true;
            }
        }

        public async Task<T> GetAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                var stored = await _cache.GetStringAsync(id, cancellationToken);

                if (!string.IsNullOrEmpty(stored))
                {
                    return JsonConvert.DeserializeObject<T>(stored);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error occurred: {ex.Message}");
            }
            return default;
        }

        public Task RemoveAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                return _cache.RemoveAsync(id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error occurred: {ex.Message}");
                return default;
            }
        }
    }
}