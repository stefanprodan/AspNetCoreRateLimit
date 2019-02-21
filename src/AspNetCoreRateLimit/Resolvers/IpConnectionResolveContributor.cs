using Microsoft.AspNetCore.Http;

namespace AspNetCoreRateLimit
{
    public class IpConnectionResolveContributor : IIpResolveContributor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IpConnectionResolveContributor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string ResolveIp()
        {
            return _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();
        }
    }
}