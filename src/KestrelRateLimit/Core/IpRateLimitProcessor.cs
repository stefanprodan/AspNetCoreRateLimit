using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KestrelRateLimit
{
    public class IpRateLimitProcessor
    {
        private RateLimitOptions _options;
        private readonly IRateLimitStore _store;
        private readonly IIpAddressParser _ipParser;
        private static readonly object _processLocker = new object();

        public IpRateLimitProcessor(RateLimitOptions options, IRateLimitStore store, IIpAddressParser ipParser)
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
                var rules = _options.EndpointRules.Where(x => pathWithVerb.Contains(x.Value.ToLowerInvariant())).ToList();
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

            // enforce ip rate limit as is most specific 
            string ipRule = null;
            if (_options.IpRules != null && _ipParser.ContainsIp(_options.IpRules.Select(x => x.Value).ToList(), identity.ClientIp, out ipRule))
            {
                var limit = _options.IpRules.First(x => x.Value == ipRule).GetLimit(rateLimitPeriod);
                if (limit > 0)
                {
                    rateLimit = limit;
                }
            }
        }

        public RateLimitEntry ProcessRequest(RequestIdentity requestIdentity, TimeSpan timeSpan, RateLimitPeriod period)
        {
            var counter = new RateLimitCounter
            {
                Timestamp = DateTime.UtcNow,
                TotalRequests = 1
            };

            var key = string.Empty;
            var id = ComputeThrottleKey(requestIdentity, period, out key);

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
                        counter = new RateLimitCounter
                        {
                            Timestamp = entry.Value.Timestamp,
                            TotalRequests = totalRequests
                        };
                    }
                }

                // stores: id (string) - timestamp (datetime) - total (long)
                _store.SaveCounter(id, counter, timeSpan);
            }

            return new RateLimitEntry
            {
                Counter = counter,
                Key = key,
                Id = id
            };
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
            if (_options.ClientWhitelist != null && _options.ClientWhitelist.Contains(requestIdentity.ClientBypassKey))
            {
                return true;
            }

            if (_options.IpWhitelist != null && _ipParser.ContainsIp(_options.IpWhitelist, requestIdentity.ClientIp))
            {
                return true;
            }

            if (_options.EnableEndpointRateLimiting)
            {
                var pathWithVerb = $"{requestIdentity.HttpVerb}:{requestIdentity.Endpoint}".ToLowerInvariant();

                if (_options.EndpointWhitelist != null
                    && _options.EndpointWhitelist.Any(x => pathWithVerb.Contains(x.ToLowerInvariant())))
                {
                    return true;
                }
            }

            return false;
        }

        public string ComputeThrottleKey(RequestIdentity requestIdentity, RateLimitPeriod period, out string key)
        {
            var keyValues = new List<string>()
                {
                    _options.GetCounterKey()
                };

            keyValues.Add(requestIdentity.ClientIp);

            if (_options.EnableEndpointRateLimiting)
            {
                keyValues.Add($"{requestIdentity.HttpVerb}:{requestIdentity.Endpoint}");
            }

            keyValues.Add(period.ToString());

            key = string.Join("_", keyValues);
            var idBytes = System.Text.Encoding.UTF8.GetBytes(key);

            byte[] hashBytes;

            using (var algorithm = System.Security.Cryptography.SHA1.Create())
            {
                hashBytes = algorithm.ComputeHash(idBytes);
            }

            return BitConverter.ToString(hashBytes).Replace("-", string.Empty);
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

        /// <summary>
        ///  If no options exists in cache save those from appsettings, if options are present in cache load those
        /// </summary>
        public RateLimitOptions GetSetOptionsInCache()
        {
            if (_options.StoreOptionsInCache)
            {
                var opt = _store.GetOptions(_options.GetOptionsKey());

                if (opt == null)
                {
                    _store.SaveOptions(_options.GetOptionsKey(), _options);
                }
                else
                {
                    _options = opt;
                }
            }
            return _options;            
        }
    }
}
