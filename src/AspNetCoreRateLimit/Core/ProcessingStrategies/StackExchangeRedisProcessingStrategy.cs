using System;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace AspNetCoreRateLimit
{
    public class StackExchangeRedisProcessingStrategy : ProcessingStrategy
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly IRateLimitConfiguration _config;

        public StackExchangeRedisProcessingStrategy(IConnectionMultiplexer connectionMultiplexer, IRateLimitCounterStore counterStore, ICounterKeyBuilder counterKeyBuilder, IRateLimitConfiguration config, RateLimitOptions options)
            : base(counterKeyBuilder, config, options)
        {
            _connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentException("IConnectionMultiplexer was null. Ensure StackExchange.Redis was successfully registered");
            _config = config;
        }


        static private readonly LuaScript _atomicIncrement = LuaScript.Prepare("local count count = redis.call(\"INCRBYFLOAT\", @key, tonumber(@delta)) if tonumber(count) == @delta then redis.call(\"EXPIRE\", @key, @timeout) end return count");

        public override async Task<RateLimitCounter> ProcessRequestAsync(ClientRequestIdentity requestIdentity, RateLimitRule rule, CancellationToken cancellationToken = default)
        {
            var counterId = BuildCounterKey(requestIdentity, rule);
            return await IncrementAsync(counterId, rule.PeriodTimespan.Value, _config.RateIncrementer);
        }

        public async Task<RateLimitCounter> IncrementAsync(string counterId, TimeSpan interval, Func<double> RateIncrementer = null)
        {
            var now = DateTime.UtcNow;
            var numberOfIntervals = now.Ticks / interval.Ticks;
            var intervalStart = new DateTime(numberOfIntervals * interval.Ticks, DateTimeKind.Utc);

            // Call the Lua script
            var count = await _connectionMultiplexer.GetDatabase().ScriptEvaluateAsync(_atomicIncrement, new { key = counterId, timeout = interval.TotalSeconds, delta = RateIncrementer?.Invoke() ?? 1D });
            return new RateLimitCounter
            {
                Count = (double)count,
                Timestamp = intervalStart
            };
        }
    }
}