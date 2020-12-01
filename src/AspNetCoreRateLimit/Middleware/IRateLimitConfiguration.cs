using System;
using System.Collections.Generic;

namespace AspNetCoreRateLimit
{
    public interface IRateLimitConfiguration
    {
        IList<IClientResolveContributor> ClientResolvers { get; }

        IList<IIpResolveContributor> IpResolvers { get; }

        ICounterKeyBuilder EndpointCounterKeyBuilder { get; }

        Func<double> RateIncrementer { get; }

        void RegisterResolvers();
    }
}