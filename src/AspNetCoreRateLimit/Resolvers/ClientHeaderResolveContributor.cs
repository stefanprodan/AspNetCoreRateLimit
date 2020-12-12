using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit
{
    public class ClientHeaderResolveContributor : IClientResolveContributor
    {
        private readonly string _headerName;

        public ClientHeaderResolveContributor(string headerName)
        {
            _headerName = headerName;
        }
        public Task<string> ResolveClientAsync(HttpContext httpContext)
        {
            string clientId = null;

            if (httpContext.Request.Headers.TryGetValue(_headerName, out var values))
            {
                clientId = values.First();
            }

            return Task.FromResult(clientId);
        }
    }
}