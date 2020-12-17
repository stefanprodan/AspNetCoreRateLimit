using StackExchange.Redis;

namespace AspNetCoreRateLimit
{
    public class StackExchangeRedisRateLimitCounterStore : StackExchangeRedisRateLimitStore<RateLimitCounter?>, IRateLimitCounterStore
    {
        public StackExchangeRedisRateLimitCounterStore(IConnectionMultiplexer connectionMultiplexer)
            : base(connectionMultiplexer)
        {
        }
    }
}