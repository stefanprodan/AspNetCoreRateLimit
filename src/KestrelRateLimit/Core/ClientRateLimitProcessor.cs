using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KestrelRateLimit
{
    public class ClientRateLimitProcessor
    {
        private readonly ClientRateLimitOptions _options;
        private readonly IRateLimitCounterStore _counterStore;
        private readonly IClientPolicyStore _policyStore;

        private static readonly object _processLocker = new object();

        public ClientRateLimitProcessor(ClientRateLimitOptions options,
           IRateLimitCounterStore counterStore,
           IClientPolicyStore policyStore)
        {
            _options = options;
            _counterStore = counterStore;
            _policyStore = policyStore;
        }

        public List<ClientRateLimit> GetMatchingRules(ClientRequestIdentity identity)
        {
            var limits = new List<ClientRateLimit>();
            var policy = _policyStore.Get($"{_options.ClientPolicyPrefix}_{identity.ClientId}");

            if (policy != null)
            {
                if (_options.EnableEndpointRateLimiting)
                {
                    // search for rules with endpoints like "*" and "*:/matching_path"
                    var pathLimits = policy.Limits.Where(l => $"*:{identity.Path}".ToLowerInvariant().Contains(l.Endpoint.ToLowerInvariant())).AsEnumerable();
                    limits.AddRange(pathLimits);

                    // search for rules with endpoints like "matching_verb:/matching_path"
                    var verbLimits = policy.Limits.Where(l => $"{identity.HttpVerb}:{identity.Path}".ToLowerInvariant().Contains(l.Endpoint.ToLowerInvariant())).AsEnumerable();
                    limits.AddRange(verbLimits);
                }
                else
                {
                    //ignore endpoint rules and search for global rules only
                    var genericLimits = policy.Limits.Where(l => l.Endpoint == "*").AsEnumerable();
                    limits.AddRange(genericLimits);
                }
            }
            
            if (_options.GlobalLimits != null)
            {
                // add global limits
                limits.AddRange(_options.GlobalLimits);
            }

            // get the most restrictive limit for each period 
            limits = limits.GroupBy(l => l.Period).Select(l => l.OrderBy(x => x.Limit)).Select(l => l.First()).ToList();

            foreach (var item in limits)
            {
                //parse period text into time spans
                item.PeriodTimespan = ConvertToTimeSpan(item.Period);
            }

            limits = limits.OrderBy(l => l.PeriodTimespan).ToList();
            if(_options.StackBlockedRequests)
            {
                limits.Reverse();   
            }

            return limits;
        }

        public string ComputeCounterKey(ClientRequestIdentity requestIdentity, ClientRateLimit rule)
        {
            var key = $"{_options.RateLimitCounterPrefix}_{requestIdentity.ClientId}_{rule.Period}";
            if(_options.EnableEndpointRateLimiting)
            {
                key += $"_{requestIdentity.HttpVerb}_{requestIdentity.Path}";
            }

            var idBytes = System.Text.Encoding.UTF8.GetBytes(key);

            byte[] hashBytes;

            using (var algorithm = System.Security.Cryptography.SHA1.Create())
            {
                hashBytes = algorithm.ComputeHash(idBytes);
            }

            return BitConverter.ToString(hashBytes).Replace("-", string.Empty);
        }
          

        public RateLimitCounter ProcessRequest(ClientRequestIdentity requestIdentity, ClientRateLimit rule)
        {
            var counter = new RateLimitCounter
            {
                Timestamp = DateTime.UtcNow,
                TotalRequests = 1
            };

            var counterId = ComputeCounterKey(requestIdentity, rule);

            // serial reads and writes
            lock (_processLocker)
            {
                var entry = _counterStore.Get(counterId);
                if (entry.HasValue)
                {
                    // entry has not expired
                    if (entry.Value.Timestamp + rule.PeriodTimespan.Value >= DateTime.UtcNow)
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
                _counterStore.Set(counterId, counter, rule.PeriodTimespan.Value);
            }

            return counter;
        }

        public bool IsWhitelisted(ClientRequestIdentity requestIdentity)
        {
            if (_options.ClientWhitelist != null && _options.ClientWhitelist.Contains(requestIdentity.ClientId))
            {
                return true;
            }

            var pathWithVerb = $"{requestIdentity.HttpVerb}:{requestIdentity.Path}".ToLowerInvariant();

            if (_options.EndpointWhitelist != null
                && _options.EndpointWhitelist.Any(x => pathWithVerb.Contains(x.ToLowerInvariant())))
            {
                return true;
            }

            return false;
        }

        public RateLimitHeaders GetRateLimitHeaders(ClientRequestIdentity requestIdentity, ClientRateLimit rule)
        {
            var headers = new RateLimitHeaders();
            var counterId = ComputeCounterKey(requestIdentity, rule);
            var entry = _counterStore.Get(counterId);
            if (entry.HasValue)
            {
                headers.Reset = (entry.Value.Timestamp + ConvertToTimeSpan(rule.Period)).ToString();
                headers.Limit = rule.Period;
                headers.Remaining = (rule.Limit - entry.Value.TotalRequests).ToString();
            }
            else
            {
                headers.Reset = (DateTime.UtcNow + ConvertToTimeSpan(rule.Period)).ToString();
                headers.Limit = rule.Period;
                headers.Remaining = rule.Limit .ToString();
            }

            return headers;
        }

        public string RetryAfterFrom(DateTime timestamp, ClientRateLimit rule)
        {
            var secondsPast = Convert.ToInt32((DateTime.UtcNow - timestamp).TotalSeconds);
            var retryAfter = Convert.ToInt32(rule.PeriodTimespan.Value.TotalSeconds);
            retryAfter = retryAfter > 1 ? retryAfter - secondsPast : 1;
            return retryAfter.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        public void SaveClientRateLimits(List<ClientRateLimitPolicy> limits)
        {
            foreach (var item in limits)
            {
                _policyStore.Set($"{_options.ClientPolicyPrefix}_{item.ClientId}", new ClientRateLimitPolicy { ClientId = item.ClientId, Limits = item.Limits });
            }
        }

        public static TimeSpan ConvertToTimeSpan(string timeSpan)
        {
            var l = timeSpan.Length - 1;
            var value = timeSpan.Substring(0, l);
            var type = timeSpan.Substring(l, 1);

            switch (type)
            {
                case "d": return TimeSpan.FromDays(double.Parse(value));
                case "h": return TimeSpan.FromHours(double.Parse(value));
                case "m": return TimeSpan.FromMinutes(double.Parse(value));
                case "s": return TimeSpan.FromSeconds(double.Parse(value));
                default: return TimeSpan.FromSeconds(double.Parse(value));
            }
        }
    }
}
