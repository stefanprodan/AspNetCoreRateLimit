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

        public void ApplyRules(RequestIdentity identity, TimeSpan timeSpan, RateLimitPeriod rateLimitPeriod, ref long rateLimit)
        {
            // apply endpoint rate limits
            if (_options.EnableEndpointRateLimiting && _options.EndpointRules != null)
            {
                var pathWithVerb = $"{identity.HttpVerb}:{identity.Endpoint}".ToLowerInvariant();
                var rules = _options.EndpointRules.Where(x => pathWithVerb == x.Value.ToLowerInvariant()).ToList();
                if (rules.Any())
                {
                    // get the lower limit from all applying rules
                    var customRate = (from r in rules let rateValue = r.GetLimit(rateLimitPeriod) select rateValue).Min();

                    if (customRate > 0)
                    {
                        rateLimit = customRate;
                    }
                }
            }

            // apply custom rate limit for clients that will override endpoint limits
            if (_options.EnableClientRateLimiting &&  _options.ClientRules != null && _options.ClientRules.Select(x => x.Value).Contains(identity.ClientKey))
            {
                var limit = _options.ClientRules.First(x => x.Value == identity.ClientKey).GetLimit(rateLimitPeriod);
                if (limit > 0)
                {
                    rateLimit = limit;
                }
            }

            // enforce ip rate limit as is most specific 
            string ipRule = null;
            if (_options.EnableIpRateLimiting && _options.IpRules != null && _ipParser.ContainsIp(_options.IpRules.Select(x => x.Value).ToList(), identity.ClientIp, out ipRule))
            {
                var limit = _options.IpRules.First(x => x.Value == ipRule).GetLimit(rateLimitPeriod);
                if (limit > 0)
                {
                    rateLimit = limit;
                }
            }
        }

        public RateLimitCounter ProcessRequest(RequestIdentity requestIdentity, TimeSpan timeSpan, RateLimitPeriod period, out string id)
        {
            var throttleCounter = new RateLimitCounter()
            {
                Timestamp = DateTime.UtcNow,
                TotalRequests = 1
            };

            id = ComputeThrottleKey(requestIdentity, period);

            // serial reads and writes
            lock (_processLocker)
            {
                var entry = _store.GetCounter(id);
                if (entry.HasValue)
                {
                    // entry has not expired
                    if (entry.Value.Timestamp + timeSpan >= DateTime.UtcNow)
                    {
                        // increment request count
                        var totalRequests = entry.Value.TotalRequests + 1;

                        // deep copy
                        throttleCounter = new RateLimitCounter
                        {
                            Timestamp = entry.Value.Timestamp,
                            TotalRequests = totalRequests
                        };
                    }
                }

                // stores: id (string) - timestamp (datetime) - total (long)
                _store.SaveCounter(id, throttleCounter, timeSpan);
            }

            return throttleCounter;
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
                var pathWithVerb = $"{requestIdentity.HttpVerb}:{requestIdentity.Endpoint}".ToLowerInvariant();

                if (_options.EndpointWhitelist != null
                    && _options.EndpointWhitelist.Any(x => pathWithVerb == x.ToLowerInvariant()))
                {
                    return true;
                }
            }

            return false;
        }

        public string ComputeThrottleKey(RequestIdentity requestIdentity, RateLimitPeriod period)
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
                keyValues.Add($"{requestIdentity.HttpVerb}:{requestIdentity.Endpoint}");
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

        public List<KeyValuePair<RateLimitPeriod, long>> RatesWithDefaults(List<KeyValuePair<RateLimitPeriod, long>> defRates)
        {
            if (!defRates.Any(x => x.Key == RateLimitPeriod.Second))
            {
                defRates.Insert(0, new KeyValuePair<RateLimitPeriod, long>(RateLimitPeriod.Second, 0));
            }

            if (!defRates.Any(x => x.Key == RateLimitPeriod.Minute))
            {
                defRates.Insert(1, new KeyValuePair<RateLimitPeriod, long>(RateLimitPeriod.Minute, 0));
            }

            if (!defRates.Any(x => x.Key == RateLimitPeriod.Hour))
            {
                defRates.Insert(2, new KeyValuePair<RateLimitPeriod, long>(RateLimitPeriod.Hour, 0));
            }

            if (!defRates.Any(x => x.Key == RateLimitPeriod.Day))
            {
                defRates.Insert(3, new KeyValuePair<RateLimitPeriod, long>(RateLimitPeriod.Day, 0));
            }

            if (!defRates.Any(x => x.Key == RateLimitPeriod.Week))
            {
                defRates.Insert(4, new KeyValuePair<RateLimitPeriod, long>(RateLimitPeriod.Week, 0));
            }

            return defRates;
        }

        public TimeSpan GetTimeSpanFromPeriod(RateLimitPeriod rateLimitPeriod)
        {
            var timeSpan = TimeSpan.FromSeconds(1);

            switch (rateLimitPeriod)
            {
                case RateLimitPeriod.Second:
                    timeSpan = TimeSpan.FromSeconds(1);
                    break;
                case RateLimitPeriod.Minute:
                    timeSpan = TimeSpan.FromMinutes(1);
                    break;
                case RateLimitPeriod.Hour:
                    timeSpan = TimeSpan.FromHours(1);
                    break;
                case RateLimitPeriod.Day:
                    timeSpan = TimeSpan.FromDays(1);
                    break;
                case RateLimitPeriod.Week:
                    timeSpan = TimeSpan.FromDays(7);
                    break;
            }

            return timeSpan;
        }
    }
}
