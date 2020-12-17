namespace AspNetCoreRateLimit
{
    public interface IProcessingStrategyFactory
    {
        ProcessingStrategy CreateProcessingStrategy(IRateLimitCounterStore counterStore, ICounterKeyBuilder counterKeyBuilder, IRateLimitConfiguration config, RateLimitOptions options);
    }
}