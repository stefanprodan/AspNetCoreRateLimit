using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit
{
    public abstract class ProcessingStrategy : IProcessingStrategy
    {
        private readonly IRateLimitConfiguration _config;

        protected ProcessingStrategy(IRateLimitConfiguration config)
        {
            _config = config;
        }

        public abstract Task<RateLimitCounter> ProcessRequestAsync(ClientRequestIdentity requestIdentity, RateLimitRule rule, ICounterKeyBuilder counterKeyBuilder, RateLimitOptions rateLimitOptions, CancellationToken cancellationToken = default);

        protected virtual string BuildCounterKey(ClientRequestIdentity requestIdentity, RateLimitRule rule, ICounterKeyBuilder counterKeyBuilder, RateLimitOptions rateLimitOptions)
        {
            var key = counterKeyBuilder.Build(requestIdentity, rule);

            if (rateLimitOptions.EnableEndpointRateLimiting && _config.EndpointCounterKeyBuilder != null)
            {
                key += _config.EndpointCounterKeyBuilder.Build(requestIdentity, rule);
            }

            var bytes = Encoding.UTF8.GetBytes(key);

            using var algorithm = new SHA1Managed();
            var hash = algorithm.ComputeHash(bytes);

            return Convert.ToBase64String(hash);
        }
    }
}