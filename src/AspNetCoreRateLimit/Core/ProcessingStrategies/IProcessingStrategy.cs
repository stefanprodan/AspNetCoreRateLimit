using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit
{
    public interface IProcessingStrategy
    {
        Task<RateLimitCounter> ProcessRequestAsync(ClientRequestIdentity requestIdentity, RateLimitRule rule, CancellationToken cancellationToken = default);
    }
}