using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace AspNetCoreRateLimit.Services
{
    public class HeaderClientRequestStore : IClientRequestStore
    {
        private ClientRateLimitOptions _options;

        public HeaderClientRequestStore(IOptions<ClientRateLimitOptions> options)
        {
            _options = options.Value;
        }
        public async Task<ClientRequestIdentity> GetClientRequestIdentityAsync(HttpContext httpContext)
        {
            var clientId = "anon";
            if (httpContext.Request.Headers.Keys.Contains(_options.ClientIdHeader, StringComparer.CurrentCultureIgnoreCase))
            {
                clientId = httpContext.Request.Headers[_options.ClientIdHeader].First();
            }

            return new ClientRequestIdentity
            {
                Path = httpContext.Request.Path.ToString().ToLowerInvariant(),
                HttpVerb = httpContext.Request.Method.ToLowerInvariant(),
                ClientId = clientId
            };
        }
    }
}