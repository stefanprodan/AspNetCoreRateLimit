namespace AspNetCoreRateLimit
{
    public interface ICounterKeyBuilder
    {
        string Build(ClientRequestIdentity requestIdentity, RateLimitRule rule);
    }
}