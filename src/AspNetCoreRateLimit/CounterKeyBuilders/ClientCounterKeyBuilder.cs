namespace AspNetCoreRateLimit
{
    public class ClientCounterKeyBuilder : ICounterKeyBuilder
    {
        private readonly ClientRateLimitOptions _options;

        public ClientCounterKeyBuilder(ClientRateLimitOptions options)
        {
            _options = options;
        }

        public string Build(ClientRequestIdentity requestIdentity, RateLimitRule rule)
        {
            return $"{_options.RateLimitCounterPrefix}_{requestIdentity.ClientId}_{rule.Period}";
        }
    }
}