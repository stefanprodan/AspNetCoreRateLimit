using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit
{
    public abstract class ProcessingStrategy : IProcessingStrategy
    {
        private readonly RateLimitOptions _options;
        private readonly ICounterKeyBuilder _counterKeyBuilder;
        private readonly IRateLimitConfiguration _config;

        public ProcessingStrategy(ICounterKeyBuilder counterKeyBuilder, IRateLimitConfiguration config, RateLimitOptions options)
            : base()
        {
            _counterKeyBuilder = counterKeyBuilder;
            _config = config;
            _options = options;
        }

        protected virtual string BuildCounterKey(ClientRequestIdentity requestIdentity, RateLimitRule rule)
        {
            var key = _counterKeyBuilder.Build(requestIdentity, rule);

            if (_options.EnableEndpointRateLimiting && _config.EndpointCounterKeyBuilder != null)
            {
                key += _config.EndpointCounterKeyBuilder.Build(requestIdentity, rule);
            }

            var bytes = Encoding.UTF8.GetBytes(key);

            using var algorithm = new SHA1Managed();
            var hash = algorithm.ComputeHash(bytes);

            return Convert.ToBase64String(hash);
        }

        public abstract Task<RateLimitCounter> ProcessRequestAsync(ClientRequestIdentity requestIdentity, RateLimitRule rule, CancellationToken cancellationToken = default);
    }
}