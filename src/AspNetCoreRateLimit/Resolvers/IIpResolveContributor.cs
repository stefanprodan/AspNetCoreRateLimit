using Microsoft.AspNetCore.Http;

namespace AspNetCoreRateLimit
{
    public interface IIpResolveContributor
    {
        string ResolveIp(HttpContext httpContext);
    }
}