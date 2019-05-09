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

        public Func<long> RateIncrementer { get; } = null;

        public RateLimitConfiguration(
            IHttpContextAccessor httpContextAccessor,
            IOptions<IpRateLimitOptions> ipOptions,
            IOptions<ClientRateLimitOptions> clientOptions)
        {
            IpRateLimitOptions = ipOptions?.Value;
            ClientRateLimitOptions = clientOptions?.Value;
            HttpContextAccessor = httpContextAccessor;

            ClientResolvers = new List<IClientResolveContributor>();
            IpResolvers = new List<IIpResolveContributor>();

            RegisterResolvers();
        }

        protected readonly IpRateLimitOptions IpRateLimitOptions;
        protected readonly ClientRateLimitOptions ClientRateLimitOptions;
        protected readonly IHttpContextAccessor HttpContextAccessor;

        protected virtual void RegisterResolvers()
        {
            if (!string.IsNullOrEmpty(ClientRateLimitOptions?.ClientIdHeader))
            {
                ClientResolvers.Add(new ClientHeaderResolveContributor(HttpContextAccessor, ClientRateLimitOptions.ClientIdHeader));
            }

            // the contributors are resolved in the order of their collection index
            if (!string.IsNullOrEmpty(IpRateLimitOptions?.RealIpHeader))
            {
                IpResolvers.Add(new IpHeaderResolveContributor(HttpContextAccessor, IpRateLimitOptions.RealIpHeader));
            }

            IpResolvers.Add(new IpConnectionResolveContributor(HttpContextAccessor));
        }
    }
}