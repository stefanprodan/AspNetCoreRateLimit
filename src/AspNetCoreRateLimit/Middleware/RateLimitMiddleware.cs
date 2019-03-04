using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit
{
    public abstract class RateLimitMiddleware<TProcessor>
        where TProcessor : IRateLimitProcessor
    {
        private readonly TProcessor _processor;
        private readonly RateLimitOptions _options;
        private readonly IRateLimitConfiguration _config;

        protected RateLimitMiddleware(
            RateLimitOptions options,
            TProcessor processor,
            IRateLimitConfiguration config)
        {
            _options = options;
            _processor = processor;
            _config = config;
        }

        public virtual async Task ThrottleAsync(HttpContext context, Func<Task> next, RateLimitRule inlineRule = null)
        {
            // check if rate limiting is enabled
            if (_options == null && inlineRule == null)
            {
                await next();
                return;
            }

            // compute identity from request
            var identity = ResolveIdentity(context);

            // check white list
            if (_processor.IsWhitelisted(identity))
            {
                await next();
                return;
            }

            var rules = inlineRule == null ? 
                await _processor.GetMatchingRulesAsync(identity, context.RequestAborted) :
                new List<RateLimitRule> { inlineRule };

            var ruleCounters = new Dictionary<RateLimitRule, RateLimitCounter>();

            foreach (var rule in rules)
            {
                // increment counter
                var counter = await _processor.ProcessRequestAsync(identity, rule, context.RequestAborted);

                if (rule.Limit > 0)
                {
                    // check if key expired
                    if (counter.Timestamp + rule.PeriodTimespan.Value < DateTime.UtcNow)
                    {
                        continue;
                    }

                    // check if limit is reached
                    if (counter.TotalRequests > rule.Limit)
                    {
                        //compute retry after value
                        var retryAfter = counter.Timestamp.RetryAfterFrom(rule);

                        // log blocked request
                        LogBlockedRequest(context, identity, counter, rule);

                        // break execution
                        await ReturnQuotaExceededResponse(context, rule, retryAfter);

                        return;
                    }
                }
                // if limit is zero or less, block the request.
                else
                {
                    // log blocked request
                    LogBlockedRequest(context, identity, counter, rule);

                    // break execution (Int32 max used to represent infinity)
                    await ReturnQuotaExceededResponse(context, rule, int.MaxValue.ToString(System.Globalization.CultureInfo.InvariantCulture));

                    return;
                }

                ruleCounters.Add(rule, counter);
            }

            // set X-Rate-Limit headers for the longest period
            if (ruleCounters.Any() && !_options.DisableRateLimitHeaders)
            {
                var ruleHeaders = ruleCounters.OrderByDescending(x => x.Key.PeriodTimespan).FirstOrDefault();

                var headers = _processor.GetRateLimitHeaders(ruleHeaders.Value, ruleHeaders.Key, context.RequestAborted);

                headers.Context = context;

                context.Response.OnStarting(SetRateLimitHeaders, state: headers);
            }

            await next();
        }

        public virtual RateLimitRule GetDeclaredRule(HttpContext httpContext, RateLimitAttribute attribute)
        {
            if (attribute == null)
            {
                return null;
            }

            return new RateLimitRule
            {
                Limit = attribute.Limit,
                Period = attribute.Period,
                Endpoint = $"{httpContext.Request.Method.ToLowerInvariant()}:{httpContext.Request.Path.ToString().ToLowerInvariant()}"
            };
        }

        public virtual ClientRequestIdentity ResolveIdentity(HttpContext httpContext)
        {
            string clientIp = null;
            string clientId = null;

            if (_config.ClientResolvers?.Any() == true)
            {
                foreach(var resolver in _config.ClientResolvers)
                {
                    clientId = resolver.ResolveClient();

                    if (!string.IsNullOrEmpty(clientId))
                    {
                        break;
                    }
                }
            }

            if (_config.IpResolvers?.Any() == true)
            {
                foreach (var resolver in _config.IpResolvers)
                {
                    clientIp = resolver.ResolveIp();

                    if (!string.IsNullOrEmpty(clientIp))
                    {
                        break;
                    }
                }
            }

            return new ClientRequestIdentity
            {
                ClientIp = clientIp,
                Path = httpContext.Request.Path.ToString().ToLowerInvariant(),
                HttpVerb = httpContext.Request.Method.ToLowerInvariant(),
                ClientId = clientId
            };
        }

        public virtual Task ReturnQuotaExceededResponse(HttpContext httpContext, RateLimitRule rule, string retryAfter)
        {
            var message = string.Format(
                _options.QuotaExceededResponse?.Content ??
                _options.QuotaExceededMessage ??
                "API calls quota exceeded! maximum admitted {0} per {1}.", rule.Limit, rule.Period, retryAfter);

            if (!_options.DisableRateLimitHeaders)
            {
                httpContext.Response.Headers["Retry-After"] = retryAfter;
            }

            httpContext.Response.StatusCode = _options.QuotaExceededResponse?.StatusCode ?? _options.HttpStatusCode;
            httpContext.Response.ContentType = _options.QuotaExceededResponse?.ContentType ?? "text/plain";

            return httpContext.Response.WriteAsync(message);
        }

        protected abstract void LogBlockedRequest(HttpContext httpContext, ClientRequestIdentity identity, RateLimitCounter counter, RateLimitRule rule);

        private Task SetRateLimitHeaders(object rateLimitHeaders)
        {
            var headers = (RateLimitHeaders)rateLimitHeaders;

            headers.Context.Response.Headers["X-Rate-Limit-Limit"] = headers.Limit;
            headers.Context.Response.Headers["X-Rate-Limit-Remaining"] = headers.Remaining;
            headers.Context.Response.Headers["X-Rate-Limit-Reset"] = headers.Reset;

            return Task.CompletedTask;
        }
    }
}