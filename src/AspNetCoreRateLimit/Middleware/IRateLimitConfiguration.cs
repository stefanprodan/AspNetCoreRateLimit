using System.Collections.Generic;

namespace AspNetCoreRateLimit
{
    public interface IRateLimitConfiguration
    {
        //bool Enabled { get; set; }

        IList<IClientResolveContributor> ClientResolvers { get; }

        IList<IIpResolveContributor> IpResolvers { get; }
    }
}
