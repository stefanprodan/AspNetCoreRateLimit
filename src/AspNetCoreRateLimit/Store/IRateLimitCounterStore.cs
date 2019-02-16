namespace AspNetCoreRateLimit
{
    public interface IRateLimitCounterStore : IRateLimitStore<RateLimitCounter?>
    {
    }
}