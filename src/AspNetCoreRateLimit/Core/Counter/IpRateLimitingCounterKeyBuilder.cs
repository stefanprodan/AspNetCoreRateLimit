namespace AspNetCoreRateLimit.Core.Counter
{
    public class IpRateLimitingCounterKeyBuilder : IBuildCounterKey
    {
        private readonly RateLimitCoreOptions _options;

        public IpRateLimitingCounterKeyBuilder(RateLimitCoreOptions options)
        {
            _options = options;
        }
        
        public string BuildCounterKey(ClientRequestIdentity requestIdentity, RateLimitRule rule)
        {
            return $"{_options.RateLimitCounterPrefix}_{requestIdentity.ClientIp}_{rule.Period}";
        }
    }
}