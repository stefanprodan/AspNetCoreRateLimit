using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace AspNetCoreRateLimit
{
    public class RateLimitConfiguration : IRateLimitConfiguration
    {
        public IList<IClientResolveContributor> ClientResolvers { get; } = new List<IClientResolveContributor>();
        public IList<IIpResolveContributor> IpResolvers { get; } = new List<IIpResolveContributor>();

        public virtual ICounterKeyBuilder EndpointCounterKeyBuilder { get; } = new PathCounterKeyBuilder();

        public virtual Func<double> RateIncrementer { get; } = () => 1;

        public RateLimitConfiguration(
            IOptions<IpRateLimitOptions> ipOptions,
            IOptions<ClientRateLimitOptions> clientOptions)
        {
            IpRateLimitOptions = ipOptions?.Value;
            ClientRateLimitOptions = clientOptions?.Value;
        }

        protected readonly IpRateLimitOptions IpRateLimitOptions;
        protected readonly ClientRateLimitOptions ClientRateLimitOptions;

        public virtual void RegisterResolvers()
        {
            string clientIdHeader = GetClientIdHeader();
            string realIpHeader = GetRealIp();

            if (clientIdHeader != null)
            {
                ClientResolvers.Add(new ClientHeaderResolveContributor(clientIdHeader));
            }

            // the contributors are resolved in the order of their collection index
            if (realIpHeader != null)
            {
                IpResolvers.Add(new IpHeaderResolveContributor(realIpHeader));
            }

            IpResolvers.Add(new IpConnectionResolveContributor());
        }

        protected string GetClientIdHeader()
        {
            return ClientRateLimitOptions?.ClientIdHeader ?? IpRateLimitOptions?.ClientIdHeader;
        }

        protected string GetRealIp()
        {
            return IpRateLimitOptions?.RealIpHeader ?? ClientRateLimitOptions?.RealIpHeader;
        }
    }
}