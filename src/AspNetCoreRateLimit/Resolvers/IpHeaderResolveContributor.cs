using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Net;

namespace AspNetCoreRateLimit
{
    public class IpHeaderResolveContributor(
        string headerName) : IIpResolveContributor
    {
        private readonly string _headerName = headerName;

        public string ResolveIp(HttpContext httpContext)
        {
            IPAddress clientIp = null;

            if (httpContext.Request.Headers.TryGetValue(_headerName, out var values))
            {
                clientIp = IpAddressUtil.ParseIp(values.Last());
            }

            return clientIp?.ToString();
        }
    }
}