using Microsoft.AspNetCore.Http;

namespace AspNetCoreRateLimit
{
    public class IpConnectionResolveContributor : IIpResolveContributor
    {

        public IpConnectionResolveContributor()
        {
            // no op
        }

        public string ResolveIp(HttpContext httpContext)
        {
            return httpContext.Connection.RemoteIpAddress?.ToString();
        }
    }
}