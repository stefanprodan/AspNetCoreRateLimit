namespace AspNetCoreRateLimit.Core.Counter
{
    public interface ICreateCounterKey
    {
        IBuildCounterKey Create(bool ipRateLimiting, RateLimitCoreOptions options);
    }
}