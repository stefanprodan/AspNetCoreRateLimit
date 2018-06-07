namespace AspNetCoreRateLimit
{
    public interface IBuildCounterKey
    {
        string BuildCounterKey(ClientRequestIdentity requestIdentity, RateLimitRule rule);
    }
}