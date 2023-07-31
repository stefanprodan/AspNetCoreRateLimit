using AspNetCoreRateLimit;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace AspNetCoreRateLimit.Redis
{
    public class RedisProcessingStrategy : ProcessingStrategy
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly IRateLimitConfiguration _config;
        private readonly ILogger<RedisProcessingStrategy> _logger;

        public RedisProcessingStrategy(IConnectionMultiplexer connectionMultiplexer, 
                                       IRateLimitConfiguration config, 
                                       ILogger<RedisProcessingStrategy> logger, 
                                       IOptions<RedisRateLimitConfiguration> optionAccessor
            )
            : base(config)
        {
            _config = config;
            _logger = logger;
            var options = optionAccessor?.Value;
            _connectionMultiplexer = options?.ConnectionMultiplexerFactory == null
                ? connectionMultiplexer ?? throw new ArgumentException(
                    "IConnectionMultiplexer was null. Ensure StackExchange.Redis was successfully registered")
                : options.ConnectionMultiplexerFactory().GetAwaiter().GetResult();

        }

        private static readonly LuaScript _atomicIncrement = LuaScript.Prepare("local count = redis.call(\"INCRBYFLOAT\", @key, tonumber(@delta)) local ttl = redis.call(\"TTL\", @key) if ttl == -1 then redis.call(\"EXPIRE\", @key, @timeout) end return count");

        public override async Task<RateLimitCounter> ProcessRequestAsync(ClientRequestIdentity requestIdentity, RateLimitRule rule, ICounterKeyBuilder counterKeyBuilder, RateLimitOptions rateLimitOptions, CancellationToken cancellationToken = default)
        {
            var counterId = BuildCounterKey(requestIdentity, rule, counterKeyBuilder, rateLimitOptions);
            return await IncrementAsync(counterId, rule.PeriodTimespan.Value, _config.RateIncrementer);
        }

        public async Task<RateLimitCounter> IncrementAsync(string counterId, TimeSpan interval, Func<double> RateIncrementer = null)
        {
            var now = DateTime.UtcNow;
            var numberOfIntervals = now.Ticks / interval.Ticks;
            var intervalStart = new DateTime(numberOfIntervals * interval.Ticks, DateTimeKind.Utc);

            _logger.LogDebug("Calling Lua script. {counterId}, {timeout}, {delta}", counterId, interval.TotalSeconds, 1D);
            var count = await _connectionMultiplexer.GetDatabase().ScriptEvaluateAsync(_atomicIncrement, new { key = new RedisKey(counterId), timeout = interval.TotalSeconds, delta = RateIncrementer?.Invoke() ?? 1D });
            return new RateLimitCounter
            {
                Count = (double)count,
                Timestamp = intervalStart
            };
        }
    }
}
