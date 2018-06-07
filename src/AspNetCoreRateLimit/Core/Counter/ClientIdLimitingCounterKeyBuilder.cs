namespace AspNetCoreRateLimit.Core.Counter
{
    public class ClientIdLimitingCounterKeyBuilder : IBuildCounterKey
    {
        private readonly RateLimitCoreOptions _options;

        public ClientIdLimitingCounterKeyBuilder(RateLimitCoreOptions options)
        {
            _options = options;
        }
        
        public string BuildCounterKey(ClientRequestIdentity requestIdentity, RateLimitRule rule)
        {
            return $"{_options.RateLimitCounterPrefix}_{requestIdentity.ClientId}_{rule.Period}";
        }
    }
}