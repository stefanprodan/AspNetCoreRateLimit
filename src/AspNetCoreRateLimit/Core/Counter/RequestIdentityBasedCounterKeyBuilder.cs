namespace AspNetCoreRateLimit
{
    public class RequestIdentityBasedCounterKeyBuilder : IBuildCounterKey
    {
        private readonly IBuildCounterKey _innerBuilder;

        public RequestIdentityBasedCounterKeyBuilder(IBuildCounterKey innerBuilder)
        {
            _innerBuilder = innerBuilder;
        }
        
        public string BuildCounterKey(ClientRequestIdentity requestIdentity, RateLimitRule rule)
        {
            string baseKey = _innerBuilder.BuildCounterKey(requestIdentity, rule);
            
            return $"{baseKey}_{requestIdentity.HttpVerb}_{requestIdentity.Path}";
        }
    }
}