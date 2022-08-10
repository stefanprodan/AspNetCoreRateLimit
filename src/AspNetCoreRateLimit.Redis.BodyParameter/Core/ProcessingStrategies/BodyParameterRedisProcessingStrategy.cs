using AspNetCoreRateLimit.Redis.BodyParameter.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AspNetCoreRateLimit.Redis.BodyParameter.Core.ProcessingStrategies
{
    public class BodyParameterRedisProcessingStrategy
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly IRateLimitConfiguration _config;
        private readonly ILogger<BodyParameterRedisProcessingStrategy> _logger;

        public BodyParameterRedisProcessingStrategy(IConnectionMultiplexer connectionMultiplexer, IRateLimitConfiguration config, ILogger<BodyParameterRedisProcessingStrategy> logger)
        {
            _connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentException("IConnectionMultiplexer was null. Ensure StackExchange.Redis was successfully registered");
            _config = config;
            _logger = logger;
        }

        private static readonly LuaScript AtomicIncrement = LuaScript.Prepare("local count = redis.call(\"INCRBYFLOAT\", @key, tonumber(@delta)) local ttl = redis.call(\"TTL\", @key) if ttl == -1 then redis.call(\"EXPIRE\", @key, @timeout) end return count");

        public BodyParameterRateLimitCounter ProcessRequest(string counterId, EndpointBodyParameterRateLimitRule rule)
        {
            return Increment(counterId, rule.PeriodTimespan.Value, _config.RateIncrementer);
        }

        private BodyParameterRateLimitCounter Increment(string counterId, TimeSpan interval, Func<double>? rateIncrementer = null)
        {
            var now = DateTime.UtcNow;
            var numberOfIntervals = now.Ticks / interval.Ticks;
            var intervalStart = new DateTime(numberOfIntervals * interval.Ticks, DateTimeKind.Utc);

            _logger.LogDebug("Calling Lua script. {counterId}, {timeout}, {delta}", counterId, interval.TotalSeconds, 1D);
            var count = _connectionMultiplexer.GetDatabase().ScriptEvaluate(AtomicIncrement, new
            {
                key = new RedisKey(counterId), timeout = interval.TotalSeconds, delta = rateIncrementer?.Invoke() ?? 1D
            });
            return new BodyParameterRateLimitCounter
            {
                Count = (double)count,
                Timestamp = intervalStart
            };
        }
    }
}
