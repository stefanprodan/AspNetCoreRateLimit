using System.Collections.Generic;

namespace AspNetCoreRateLimit
{
    public interface IRateLimitProcessor
    {
        IEnumerable<RateLimitRule> GetMatchingRules(ClientRequestIdentity identity);

        RateLimitHeaders GetRateLimitHeaders(ClientRequestIdentity requestIdentity, RateLimitRule rule);

        bool IsWhitelisted(ClientRequestIdentity requestIdentity);

        RateLimitCounter ProcessRequest(ClientRequestIdentity requestIdentity, RateLimitRule rule);
    }
}