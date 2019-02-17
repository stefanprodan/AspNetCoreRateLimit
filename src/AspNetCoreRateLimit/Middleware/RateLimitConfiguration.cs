using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace AspNetCoreRateLimit
{
    public class RateLimitConfiguration : IRateLimitConfiguration
    {
        //public bool Enabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IList<IClientResolveContributor> ClientResolvers { get; }

        public IList<IIpResolveContributor> IpResolvers { get; }

        public RateLimitConfiguration(
            IHttpContextAccessor httpContextAccessor,
            IOptions<IpRateLimitOptions> ipOptions,
            IOptions<ClientRateLimitOptions> clientOptions)
        {
            ClientResolvers = new List<IClientResolveContributor>();

            if (!string.IsNullOrEmpty(clientOptions?.Value.ClientIdHeader))
            {
                ClientResolvers.Add(new ClientHeaderResolveContributor(httpContextAccessor, clientOptions.Value.ClientIdHeader));
            }

            IpResolvers = new List<IIpResolveContributor>();

            // the contributors are resolved in the order of their collection index
            if (!string.IsNullOrEmpty(ipOptions?.Value.RealIpHeader))
            {
                IpResolvers.Add(new IpHeaderResolveContributor(httpContextAccessor, ipOptions.Value.RealIpHeader));
            }

            IpResolvers.Add(new IpConnectionResolveContributor(httpContextAccessor));
        }
    }
}