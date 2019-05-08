using Microsoft.AspNetCore.Http;
using System.Linq;

namespace AspNetCoreRateLimit
{
    public class MPHeaderResolveContributor : IMPResolveContributor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _headerName;

        public MPHeaderResolveContributor(
            IHttpContextAccessor httpContextAccessor,
            string headerName)
        {
            _httpContextAccessor = httpContextAccessor;
            _headerName = headerName;
        }

        //Resolve the header and return its value
        public long ResolveMP()
        {
            long MPValue = 0;
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext.Request.Headers.TryGetValue(_headerName, out var values))
            {
                MPValue = System.Convert.ToInt64(values.First());
            }

            return MPValue;
        }
    }
}