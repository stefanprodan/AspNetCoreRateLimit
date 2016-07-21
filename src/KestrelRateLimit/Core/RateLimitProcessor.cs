using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KestrelRateLimit
{
    public class RateLimitProcessor
    {
        private readonly RateLimitOptions _options;
        private readonly IRateLimitStore _store;
        private readonly IIPAddressParser _ipParser;
        private static readonly object _processLocker = new object();

        public RateLimitProcessor(RateLimitOptions options, IRateLimitStore store, IIPAddressParser ipParser)
        {
            _options = options;
            _store = store;
            _ipParser = ipParser;
        }

        public string RetryAfterFrom(DateTime timestamp, RateLimitPeriod period)
        {
            var secondsPast = Convert.ToInt32((DateTime.UtcNow - timestamp).TotalSeconds);
            var retryAfter = 1;
            switch (period)
            {
                case RateLimitPeriod.Minute:
                    retryAfter = 60;
                    break;
                case RateLimitPeriod.Hour:
                    retryAfter = 60 * 60;
                    break;
                case RateLimitPeriod.Day:
                    retryAfter = 60 * 60 * 24;
                    break;
                case RateLimitPeriod.Week:
                    retryAfter = 60 * 60 * 24 * 7;
                    break;
            }
            retryAfter = retryAfter > 1 ? retryAfter - secondsPast : 1;
            return retryAfter.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        public bool IsWhitelisted(RequestIdentity requestIdentity)
        {
            if (_options.EnableIpRateLimiting)
            {
                if (_options.IpWhitelist != null && _ipParser.ContainsIp(_options.IpWhitelist, requestIdentity.ClientIp))
                {
                    return true;
                }
            }

            if (_options.EnableClientRateLimiting)
            {
                if (_options.ClientWhitelist != null && _options.ClientWhitelist.Contains(requestIdentity.ClientKey))
                {
                    return true;
                }
            }

            if (_options.EnableEndpointRateLimiting)
            {
                if (_options.EndpointWhitelist != null
                    && _options.EndpointWhitelist.Any(x => requestIdentity.Endpoint.Contains(x.ToLowerInvariant())))
                {
                    return true;
                }
            }

            return false;
        }

        internal string ComputeThrottleKey(RequestIdentity requestIdentity, RateLimitPeriod period)
        {
            var keyValues = new List<string>()
                {
                    _options.GetCounterKey()
                };

            if (_options.EnableIpRateLimiting)
            {
                keyValues.Add(requestIdentity.ClientIp);
            }

            if (_options.EnableClientRateLimiting)
            {
                keyValues.Add(requestIdentity.ClientKey);
            }

            if (_options.EnableEndpointRateLimiting)
            {
                keyValues.Add(requestIdentity.Endpoint);
            }

            keyValues.Add(period.ToString());

            var id = string.Join("_", keyValues);
            var idBytes = System.Text.Encoding.UTF8.GetBytes(id);

            byte[] hashBytes;

            using (var algorithm = System.Security.Cryptography.SHA1.Create())
            {
                hashBytes = algorithm.ComputeHash(idBytes);
            }

            var hex = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
            return hex;
        }
    }
}
