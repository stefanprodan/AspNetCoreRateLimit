using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit.Redis
{
    public class RedisProcessingStrategy : ProcessingStrategy
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly IRateLimitConfiguration _config;
        private readonly ILogger<RedisProcessingStrategy> _logger;

        public RedisProcessingStrategy(IConnectionMultiplexer connectionMultiplexer, IRateLimitConfiguration config, ILogger<RedisProcessingStrategy> logger)
            : base(config)
        {
            _connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentException("IConnectionMultiplexer was null. Ensure StackExchange.Redis was successfully registered");
            _config = config;
            _logger = logger;
        }

        static private readonly LuaScript _atomicIncrement = LuaScript.Prepare("local count = redis.call(\"INCRBYFLOAT\", @key, tonumber(@delta)) local ttl = redis.call(\"TTL\", @key) if ttl == -1 then redis.call(\"EXPIRE\", @key, @timeout) end return { 'count', count, 'ttl', ttl }");
        public override async Task<RateLimitCounter> ProcessRequestAsync(ClientRequestIdentity requestIdentity, RateLimitRule rule, ICounterKeyBuilder counterKeyBuilder, RateLimitOptions rateLimitOptions, CancellationToken cancellationToken = default)
        {
            var counterId = BuildCounterKey(requestIdentity, rule, counterKeyBuilder, rateLimitOptions);
            return await IncrementAsync(counterId, rule.PeriodTimespan.Value, _config.RateIncrementer);
        }

        public async Task<RateLimitCounter> IncrementAsync(string counterId, TimeSpan interval, Func<double> RateIncrementer = null)
        {
            _logger.LogDebug("Calling Lua script. {counterId}, {timeout}, {delta}", counterId, interval.TotalSeconds, 1D);
            var cacheStart = DateTime.UtcNow;
            var cached = await _connectionMultiplexer.GetDatabase().ScriptEvaluateAsync(_atomicIncrement, new { key = new RedisKey(counterId), timeout = interval.TotalSeconds, delta = RateIncrementer?.Invoke() ?? 1D });
            var responseDict = cached.ToDictionary();
            var ttlSeconds = (int)responseDict["ttl"];
            if (ttlSeconds != -1)
                cacheStart = cacheStart.Add(-interval).AddSeconds(ttlSeconds); // Subtract the amount of seconds the interval adds, then add the amount of seconds still left to live.
            var count = (double)responseDict["count"];
            return new RateLimitCounter
            {
                Count = count,
                Timestamp = cacheStart
            };
        }
    }
}
