using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit
{
    public interface IRateLimitStore<T>
    {
        Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);
        Task<T> GetAsync(string id, CancellationToken cancellationToken = default);
        Task RemoveAsync(string id, CancellationToken cancellationToken = default);
        Task SetAsync(string id, T entry, TimeSpan? expirationTime = null, CancellationToken cancellationToken = default);
    }
}