namespace AspNetCoreRateLimit
{
    public class EndpointRuleBasedCounterKeyBuilder : IBuildCounterKey
    {
        private readonly IBuildCounterKey _innerBuilder;

        public EndpointRuleBasedCounterKeyBuilder(IBuildCounterKey innerBuilder)
        {
            _innerBuilder = innerBuilder;
        }
        
        public string BuildCounterKey(ClientRequestIdentity requestIdentity, RateLimitRule rule)
        {
            string baseKey = _innerBuilder.BuildCounterKey(requestIdentity, rule);
            
            return $"{baseKey}_{rule.Endpoint}";
        }
    }
}