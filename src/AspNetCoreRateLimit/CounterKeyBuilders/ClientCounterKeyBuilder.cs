namespace AspNetCoreRateLimit
{
    public class ClientCounterKeyBuilder(ClientRateLimitOptions options) : ICounterKeyBuilder
    {
        private readonly ClientRateLimitOptions _options = options;

        public string Build(ClientRequestIdentity requestIdentity, RateLimitRule rule)
        {
            return $"{_options.RateLimitCounterPrefix}_{requestIdentity.ClientId}_{rule.Period}";
        }
    }
}