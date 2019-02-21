using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Net;

namespace AspNetCoreRateLimit
{
    public class IpHeaderResolveContributor : IIpResolveContributor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _headerName;

        public IpHeaderResolveContributor(
            IHttpContextAccessor httpContextAccessor,
            string headerName)
        {
            _httpContextAccessor = httpContextAccessor;
            _headerName = headerName;
        }

        public string ResolveIp()
        {
            IPAddress clientIp = null;

            var httpContent = _httpContextAccessor.HttpContext;

            if (httpContent.Request.Headers.TryGetValue(_headerName, out var values))
            {
                clientIp = IpAddressUtil.ParseIp(values.Last());
            }

            return clientIp?.ToString();
        }
    }
}