using System;
using StackExchange.Redis;

namespace AspNetCoreRateLimit
{

    public class ProcessingStrategyFactory : IProcessingStrategyFactory
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public ProcessingStrategyFactory(IConnectionMultiplexer connectionMultiplexer = null)
        {
            _connectionMultiplexer = connectionMultiplexer;
        }

        public ProcessingStrategy CreateProcessingStrategy(IRateLimitCounterStore counterStore, ICounterKeyBuilder counterKeyBuilder, IRateLimitConfiguration config, RateLimitOptions options)
        {
            return counterStore switch
            {
                MemoryCacheRateLimitCounterStore => new AsyncKeyLockProcessingStrategy(counterStore, counterKeyBuilder, config, options),
                DistributedCacheRateLimitCounterStore => new AsyncKeyLockProcessingStrategy(counterStore, counterKeyBuilder, config, options),
                StackExchangeRedisRateLimitCounterStore => new StackExchangeRedisProcessingStrategy(_connectionMultiplexer, counterStore, counterKeyBuilder, config, options),
                _ => throw new ArgumentException("Unsupported instance of IRateLimitCounterStore provided")
            };
        }
    }
}