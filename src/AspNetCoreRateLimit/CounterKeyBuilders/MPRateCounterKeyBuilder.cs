using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCoreRateLimit
{
    public class MPRateCounterKeyBuilder : ICounterKeyBuilder
    {
        private readonly MPRateLimitOptions _options;

        public MPRateCounterKeyBuilder(MPRateLimitOptions options)
        {
            _options = options;
        }

        public string Build(ClientRequestIdentity requestIdentity, RateLimitRule rule)
        {
            return $"{_options.MPRatePolicyPrefix}_{requestIdentity.ClientId}_{rule.Period}";
        }
    }
}
