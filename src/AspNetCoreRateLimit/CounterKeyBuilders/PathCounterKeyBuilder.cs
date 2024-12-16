namespace AspNetCoreRateLimit
{
    public class PathCounterKeyBuilder : ICounterKeyBuilder
    {
        public string Build(ClientRequestIdentity requestIdentity, RateLimitRule rule)
        {
            switch (rule.ConterKeyMode) {
                case EPConterKeyMode.VerbAndPath:
                    return $"_VP_{requestIdentity.HttpVerb}_{requestIdentity.Path}";
                case EPConterKeyMode.Path:
                    return $"_P_{requestIdentity.Path}";
                case EPConterKeyMode.VerbAndRule:
                    return $"_VE_{requestIdentity.HttpVerb}_{rule.Endpoint}";
                default:
                    return $"_E_{rule.Endpoint}";
            }
        }
    }
}
