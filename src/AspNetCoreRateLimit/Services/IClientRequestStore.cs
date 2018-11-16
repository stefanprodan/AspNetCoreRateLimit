using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AspNetCoreRateLimit.Services
{
    public interface IClientRequestStore
    {
        Task<ClientRequestIdentity> GetClientRequestIdentityAsync(HttpContext context);
    }
}