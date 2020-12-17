using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit
{
    public class StackExchangeRedisRateLimitStore<T> : IRateLimitStore<T>
    {
        private readonly IConnectionMultiplexer _redis;

        public StackExchangeRedisRateLimitStore(IConnectionMultiplexer redis)
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        }

        public async Task SetAsync(string id, T entry, TimeSpan? expirationTime = null, CancellationToken cancellationToken = default)
        {
            // Throw an exception if the key could not be set
            if (!await _redis.GetDatabase().StringSetAsync(id, JsonConvert.SerializeObject(entry), expirationTime))
            {
                throw new ExternalException($"Failed to set key {id}");
            }
        }

        public Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
        {
            return _redis.GetDatabase().KeyExistsAsync(id);
        }

        public async Task<T> GetAsync(string id, CancellationToken cancellationToken = default)
        {
            var stored = await _redis.GetDatabase().StringGetAsync(id);
            if (stored.HasValue)
            {
                return JsonConvert.DeserializeObject<T>(stored.ToString());
            }

            return default;
        }

        public Task RemoveAsync(string id, CancellationToken cancellationToken = default)
        {
            // Don't throw an exception if the key doesn't exist
            return _redis.GetDatabase().KeyDeleteAsync(id);
        }
    }
}