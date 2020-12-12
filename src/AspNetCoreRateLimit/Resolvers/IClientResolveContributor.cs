using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit
{
    public interface IClientResolveContributor
    {
        Task<string> ResolveClientAsync(HttpContext httpContext);
    }
}
