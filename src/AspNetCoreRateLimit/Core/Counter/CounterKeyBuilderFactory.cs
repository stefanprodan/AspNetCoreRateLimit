using System;
using AspNetCoreRateLimit.Core.Counter;

namespace AspNetCoreRateLimit
{
    public class CounterKeyBuilderFactory : ICreateCounterKey
    {
        public IBuildCounterKey Create(bool ipRateLimiting, RateLimitCoreOptions options)
        {
            IBuildCounterKey builder = null;

            if (ipRateLimiting)
            {
                builder = new IpRateLimitingCounterKeyBuilder(options);
            }
            else
            {
                builder = new ClientIdLimitingCounterKeyBuilder(options);
            }


            if (options.EnableEndpointRateLimiting)
            {
                if (options.CounterKeyBuilder.Equals("endpoint", StringComparison.OrdinalIgnoreCase))
                {
                    builder = new EndpointRuleBasedCounterKeyBuilder(builder);
                }
                else
                {
                    builder = new RequestIdentityBasedCounterKeyBuilder(builder);
                }
            }



            return builder;
        }
    }
}