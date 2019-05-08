using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit
{
    public abstract class RateLimitProcessor
    {
        private readonly RateLimitOptions _options;
        private readonly IRateLimitCounterStore _counterStore;
        private readonly ICounterKeyBuilder _counterKeyBuilder;
        private readonly IRateLimitConfiguration _config;

        protected RateLimitProcessor(
           RateLimitOptions options,
           IRateLimitCounterStore counterStore,
           ICounterKeyBuilder counterKeyBuilder,
           IRateLimitConfiguration config)
        {
            _options = options;
            _counterStore = counterStore;
            _counterKeyBuilder = counterKeyBuilder;
            _config = config;
        }

        /// The key-lock used for limiting requests.
        private static readonly AsyncKeyLock AsyncLock = new AsyncKeyLock();

        public virtual bool IsWhitelisted(ClientRequestIdentity requestIdentity)
        {
            if (_options.ClientWhitelist != null && _options.ClientWhitelist.Contains(requestIdentity.ClientId))
            {
                return true;
            }

            if (_options.EndpointWhitelist != null && _options.EndpointWhitelist.Any())
            {
                if (_options.EndpointWhitelist.Any(x => $"{requestIdentity.HttpVerb}:{requestIdentity.Path}".IsWildcardMatch(x)) ||
                    _options.EndpointWhitelist.Any(x => $"*:{requestIdentity.Path}".IsWildcardMatch(x)))
                    return true;
            }

            return false;
        }

        public virtual async Task<RateLimitCounter> ProcessRequestAsync(ClientRequestIdentity requestIdentity, RateLimitRule rule, CancellationToken cancellationToken = default)
        {
            var counter = new RateLimitCounter
            {
                Timestamp = DateTime.UtcNow,
                TotalRequests = 1,
                TotalMPRequests = requestIdentity.MPValue
            };

            var counterId = BuildCounterKey(requestIdentity, rule);

            // serial reads and writes on same key
            using (await AsyncLock.WriterLockAsync(counterId).ConfigureAwait(false))
            {
                var entry = await _counterStore.GetAsync(counterId, cancellationToken);

                long totalRequests = 0;
                long totalMPRequests = 0;

                if (entry.HasValue)
                {
                    // entry has not expired
                    if (entry.Value.Timestamp + rule.PeriodTimespan.Value >= DateTime.UtcNow)
                    {
                        // increment MP request count
                        if (Convert.ToInt64(0) != requestIdentity.MPValue)
                        {
                            totalMPRequests = entry.Value.TotalMPRequests + requestIdentity.MPValue;
                        }
                        else
                        {
                            // increment request count
                            totalRequests = entry.Value.TotalRequests + 1;
                        }

                        // deep copy
                        counter = new RateLimitCounter
                        {
                            Timestamp = entry.Value.Timestamp,
                            TotalRequests = totalRequests,
                            TotalMPRequests = totalMPRequests
                        };
                    }
                }

                // stores: id (string) - timestamp (datetime) - total_requests (long)
                await _counterStore.SetAsync(counterId, counter, rule.PeriodTimespan.Value, cancellationToken);
            }

            return counter;
        }

        public virtual RateLimitHeaders GetRateLimitHeaders(RateLimitCounter? counter, RateLimitRule rule, CancellationToken cancellationToken = default)
        {
            var headers = new RateLimitHeaders();

            long remaining;
            DateTime reset;

            if (counter.HasValue)
            {
                reset = counter.Value.Timestamp + (rule.PeriodTimespan ?? rule.Period.ToTimeSpan());
                remaining = rule.Limit - (counter.Value.TotalMPRequests.Equals(0) ? counter.Value.TotalRequests : counter.Value.TotalMPRequests);
            }
            else
            {
                reset = DateTime.UtcNow + (rule.PeriodTimespan ?? rule.Period.ToTimeSpan());
                remaining = rule.Limit;
            }

            headers.Reset = reset.ToUniversalTime().ToString("o", DateTimeFormatInfo.InvariantInfo);
            headers.Limit = rule.Period;
            headers.Remaining = remaining.ToString();

            return headers;
        }

        protected virtual string BuildCounterKey(ClientRequestIdentity requestIdentity, RateLimitRule rule)
        {
            var key = _counterKeyBuilder.Build(requestIdentity, rule);

            if (_options.EnableEndpointRateLimiting && _config.EndpointCounterKeyBuilder != null)
            {
                key += _config.EndpointCounterKeyBuilder.Build(requestIdentity, rule);
            }

            var bytes = Encoding.UTF8.GetBytes(key);

            using (var algorithm = new SHA1Managed())
            {
                var hash = algorithm.ComputeHash(bytes);

                return Convert.ToBase64String(hash);
            }
        }

        protected virtual List<RateLimitRule> GetMatchingRules(ClientRequestIdentity identity, List<RateLimitRule> rules = null)
        {
            var limits = new List<RateLimitRule>();

            if (rules?.Any() == true)
            {
                if (_options.EnableEndpointRateLimiting)
                {
                    // search for rules with endpoints like "*" and "*:/matching_path"
                    var pathLimits = rules.Where(r => $"*:{identity.Path}".IsWildcardMatch(r.Endpoint));
                    limits.AddRange(pathLimits);

                    // search for rules with endpoints like "matching_verb:/matching_path"
                    var verbLimits = rules.Where(r => $"{identity.HttpVerb}:{identity.Path}".IsWildcardMatch(r.Endpoint));
                    limits.AddRange(verbLimits);
                }
                else
                {
                    // ignore endpoint rules and search for global rules only
                    var genericLimits = rules.Where(r => r.Endpoint == "*");
                    limits.AddRange(genericLimits);
                }

                // get the most restrictive limit for each period 
                limits = limits.GroupBy(l => l.Period).Select(l => l.OrderBy(x => x.Limit)).Select(l => l.First()).ToList();
            }

            // search for matching general rules
            if (_options.GeneralRules != null)
            {
                var matchingGeneralLimits = new List<RateLimitRule>();

                if (_options.EnableEndpointRateLimiting)
                {
                    // search for rules with endpoints like "*" and "*:/matching_path" in general rules
                    var pathLimits = _options.GeneralRules.Where(r => $"*:{identity.Path}".IsWildcardMatch(r.Endpoint));
                    matchingGeneralLimits.AddRange(pathLimits);

                    // search for rules with endpoints like "matching_verb:/matching_path" in general rules
                    var verbLimits = _options.GeneralRules.Where(r => $"{identity.HttpVerb}:{identity.Path}".IsWildcardMatch(r.Endpoint));
                    matchingGeneralLimits.AddRange(verbLimits);
                }
                else
                {
                    //ignore endpoint rules and search for global rules in general rules
                    var genericLimits = _options.GeneralRules.Where(r => r.Endpoint == "*");
                    matchingGeneralLimits.AddRange(genericLimits);
                }

                // get the most restrictive general limit for each period 
                var generalLimits = matchingGeneralLimits
                    .GroupBy(l => l.Period)
                    .Select(l => l.OrderBy(x => x.Limit).ThenBy(x => x.Endpoint))
                    .Select(l => l.First())
                    .ToList();

                foreach (var generalLimit in generalLimits)
                {
                    // add general rule if no specific rule is declared for the specified period
                    if (!limits.Exists(l => l.Period == generalLimit.Period))
                    {
                        limits.Add(generalLimit);
                    }
                }
            }

            foreach (var item in limits)
            {
                // parse period text into time spans
                item.PeriodTimespan = item.Period.ToTimeSpan();
            }

            limits = limits.OrderBy(l => l.PeriodTimespan).ToList();

            if (_options.StackBlockedRequests)
            {
                limits.Reverse();
            }

            return limits;
        }
    }
}