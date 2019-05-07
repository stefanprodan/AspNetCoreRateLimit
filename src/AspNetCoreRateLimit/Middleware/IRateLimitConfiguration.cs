using System.Collections.Generic;

namespace AspNetCoreRateLimit
{
    public interface IRateLimitConfiguration
    {
        IList<IClientResolveContributor> ClientResolvers { get; }

        IList<IIpResolveContributor> IpResolvers { get; }

        IList<IMPResolveContributor> MPResolvers { get; }

        ICounterKeyBuilder EndpointCounterKeyBuilder { get; }
    }
}