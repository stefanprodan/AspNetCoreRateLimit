using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit
{
    public class ClientHeaderResolveContributor(string headerName) : IClientResolveContributor
    {
        private readonly string _headerName = headerName;

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