namespace AspNetCoreRateLimit
{
    public class IpCounterKeyBuilder(IpRateLimitOptions options) : ICounterKeyBuilder
    {
        private readonly IpRateLimitOptions _options = options;

        public string Build(ClientRequestIdentity requestIdentity, RateLimitRule rule)
        {
            return $"{_options.RateLimitCounterPrefix}_{requestIdentity.ClientIp}_{rule.Period}";
        }
    }
}