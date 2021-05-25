using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit
{
    public class AsyncKeyLockProcessingStrategy : ProcessingStrategy
    {
        private readonly IRateLimitCounterStore _counterStore;
        private readonly IRateLimitConfiguration _config;

        public AsyncKeyLockProcessingStrategy(IRateLimitCounterStore counterStore, IRateLimitConfiguration config)
            : base(config)
        {
            _counterStore = counterStore;
            _config = config;
        }

        /// The key-lock used for limiting requests.
        private static readonly AsyncKeyLock AsyncLock = new AsyncKeyLock();

        public override async Task<RateLimitCounter> ProcessRequestAsync(ClientRequestIdentity requestIdentity, RateLimitRule rule, ICounterKeyBuilder counterKeyBuilder, RateLimitOptions rateLimitOptions, CancellationToken cancellationToken = default)
        {
            var counter = new RateLimitCounter
            {
                Timestamp = DateTime.UtcNow,
                Count = 1
            };

            var counterId = BuildCounterKey(requestIdentity, rule, counterKeyBuilder, rateLimitOptions);

            // serial reads and writes on same key
            using (await AsyncLock.WriterLockAsync(counterId).ConfigureAwait(false))
            {
                var entry = await _counterStore.GetAsync(counterId, cancellationToken);

                if (entry.HasValue)
                {
                    // entry has not expired
                    if (entry.Value.Timestamp + rule.PeriodTimespan.Value >= DateTime.UtcNow)
                    {
                        // increment request count
                        var totalCount = entry.Value.Count + _config.RateIncrementer?.Invoke() ?? 1;

                        // deep copy
                        counter = new RateLimitCounter
                        {
                            Timestamp = entry.Value.Timestamp,
                            Count = totalCount
                        };
                    }
                }

                // stores: id (string) - timestamp (datetime) - total_requests (long)
                await _counterStore.SetAsync(counterId, counter, rule.PeriodTimespan.Value, cancellationToken);
            }

            return counter;
        }
    }
}