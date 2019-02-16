using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit
{
    public abstract class RateLimitProcessor
    {
        private readonly RateLimitOptions _options;
        private readonly IRateLimitCounterStore _counterStore;

        protected RateLimitProcessor(
           RateLimitOptions options,
           IRateLimitCounterStore counterStore)
        {
            _options = options;
            _counterStore = counterStore;
        }

        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1);

        public virtual bool IsWhitelisted(ClientRequestIdentity requestIdentity)
        {
            if (_options.ClientWhitelist != null && _options.ClientWhitelist.Contains(requestIdentity.ClientId))
            {
                return true;
            }

            if (_options.EndpointWhitelist != null && _options.EndpointWhitelist.Any())
            {
                if (_options.EndpointWhitelist.Any(x => $"{requestIdentity.HttpVerb}:{requestIdentity.Path}".ContainsIgnoreCase(x)) ||
                    _options.EndpointWhitelist.Any(x => $"*:{requestIdentity.Path}".ContainsIgnoreCase(x)))
                    return true;
            }

            return false;
        }

        public async Task<RateLimitCounter> ProcessRequestAsync(ClientRequestIdentity requestIdentity, RateLimitRule rule, CancellationToken cancellationToken = default)
        {
            var counter = new RateLimitCounter
            {
                Timestamp = DateTime.UtcNow,
                TotalRequests = 1
            };

            var counterId = ComputeCounterKey(requestIdentity, rule);

            // serial reads and writes
            await Semaphore.WaitAsync(cancellationToken);

            try
            {
                var entry = await _counterStore.GetAsync(counterId, cancellationToken);

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

                // stores: id (string) - timestamp (datetime) - total_requests (long)
                await _counterStore.SetAsync(counterId, counter, rule.PeriodTimespan.Value, cancellationToken);
            }
            finally
            {
                Semaphore.Release();
            }

            return counter;
        }

        public async Task<RateLimitHeaders> GetRateLimitHeadersAsync(ClientRequestIdentity requestIdentity, RateLimitRule rule, CancellationToken cancellationToken = default)
        {
            var headers = new RateLimitHeaders();
            var counterId = ComputeCounterKey(requestIdentity, rule);
            var entry = await _counterStore.GetAsync(counterId, cancellationToken);

            long remaining;
            DateTime reset;

            if (entry.HasValue)
            {
                reset = entry.Value.Timestamp + rule.Period.ToTimeSpan();
                remaining = rule.Limit - entry.Value.TotalRequests;
            }
            else
            {
                reset = DateTime.UtcNow + rule.Period.ToTimeSpan();
                remaining = rule.Limit;
            }

            headers.Reset = reset.ToUniversalTime().ToString("o", DateTimeFormatInfo.InvariantInfo);
            headers.Limit = rule.Period;
            headers.Remaining = remaining.ToString();

            return headers;
        }

        protected abstract string GetCounterKey(ClientRequestIdentity requestIdentity, RateLimitRule rule);

        protected string ComputeCounterKey(ClientRequestIdentity requestIdentity, RateLimitRule rule)
        {
            var key = GetCounterKey(requestIdentity, rule);

            if (_options.EnableEndpointRateLimiting)
            {
                key += $"_{requestIdentity.HttpVerb}_{requestIdentity.Path}";

                // TODO: consider using the rule endpoint as key, this will allow to rate limit /api/values/1 and api/values/2 under same counter
                //key += $"_{rule.Endpoint}";
            }

            var idBytes = System.Text.Encoding.UTF8.GetBytes(key);

            byte[] hashBytes;

            using (var algorithm = System.Security.Cryptography.SHA1.Create())
            {
                hashBytes = algorithm.ComputeHash(idBytes);
            }

            return BitConverter.ToString(hashBytes).Replace("-", string.Empty);
        }

        protected List<RateLimitRule> GetMatchingRules(ClientRequestIdentity identity, List<RateLimitRule> rules)
        {
            var limits = new List<RateLimitRule>();

            if (_options.EnableEndpointRateLimiting)
            {
                // search for rules with endpoints like "*" and "*:/matching_path"
                var pathLimits = rules.Where(l => $"*:{identity.Path}".ContainsIgnoreCase(l.Endpoint)).AsEnumerable();
                limits.AddRange(pathLimits);

                // search for rules with endpoints like "matching_verb:/matching_path"
                var verbLimits = rules.Where(l => $"{identity.HttpVerb}:{identity.Path}".ContainsIgnoreCase(l.Endpoint)).AsEnumerable();
                limits.AddRange(verbLimits);
            }
            else
            {
                //ignore endpoint rules and search for global rules only
                var genericLimits = rules.Where(l => l.Endpoint == "*").AsEnumerable();
                limits.AddRange(genericLimits);
            }

            // get the most restrictive limit for each period 
            limits = limits.GroupBy(l => l.Period).Select(l => l.OrderBy(x => x.Limit)).Select(l => l.First()).ToList();

            // search for matching general rules
            if (_options.GeneralRules != null)
            {
                var matchingGeneralLimits = new List<RateLimitRule>();
                if (_options.EnableEndpointRateLimiting)
                {
                    // search for rules with endpoints like "*" and "*:/matching_path" in general rules
                    var pathLimits = _options.GeneralRules.Where(l => $"*:{identity.Path}".ContainsIgnoreCase(l.Endpoint)).AsEnumerable();
                    matchingGeneralLimits.AddRange(pathLimits);

                    // search for rules with endpoints like "matching_verb:/matching_path" in general rules
                    var verbLimits = _options.GeneralRules.Where(l => $"{identity.HttpVerb}:{identity.Path}".ContainsIgnoreCase(l.Endpoint)).AsEnumerable();
                    matchingGeneralLimits.AddRange(verbLimits);
                }
                else
                {
                    //ignore endpoint rules and search for global rules in general rules
                    var genericLimits = _options.GeneralRules.Where(l => l.Endpoint == "*").AsEnumerable();
                    matchingGeneralLimits.AddRange(genericLimits);
                }

                // get the most restrictive general limit for each period 
                var generalLimits = matchingGeneralLimits.GroupBy(l => l.Period).Select(l => l.OrderBy(x => x.Limit)).Select(l => l.First()).ToList();

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
                //parse period text into time spans
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