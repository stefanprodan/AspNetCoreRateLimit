using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit
{
    public abstract class RateLimitMiddleware<TProcessor>
        where TProcessor : IRateLimitProcessor
    {
        private readonly RequestDelegate _next;
        private readonly TProcessor _processor;
        private readonly RateLimitOptions _options;
        private readonly IRateLimitConfiguration _config;

        protected RateLimitMiddleware(
            RequestDelegate next,
            RateLimitOptions options,
            TProcessor processor,
            IRateLimitConfiguration config)
        {
            _next = next;
            _options = options;
            _processor = processor;
            _config = config;
        }

        public async Task Invoke(HttpContext context)
        {
            // check if rate limiting is enabled
            if (_options == null)
            {
                await _next.Invoke(context);
                return;
            }

            // compute identity from request
            var identity = ResolveIdentity(context);

            // check white list
            if (_processor.IsWhitelisted(identity))
            {
                await _next.Invoke(context);
                return;
            }

            var rules = await _processor.GetMatchingRulesAsync(identity, context.RequestAborted);

            foreach (var rule in rules)
            {
                if (rule.Limit > 0)
                {
                    // increment counter
                    var counter = await _processor.ProcessRequestAsync(identity, rule, context.RequestAborted);

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
                    // process request count
                    var counter = await _processor.ProcessRequestAsync(identity, rule, context.RequestAborted);

                    // log blocked request
                    LogBlockedRequest(context, identity, counter, rule);

                    // break execution (Int32 max used to represent infinity)
                    await ReturnQuotaExceededResponse(context, rule, int.MaxValue.ToString(System.Globalization.CultureInfo.InvariantCulture));

                    return;
                }
            }

            // set X-Rate-Limit headers for the longest period
            if (rules.Any() && !_options.DisableRateLimitHeaders)
            {
                var rule = rules.OrderByDescending(x => x.PeriodTimespan.Value).First();
                var headers = await _processor.GetRateLimitHeadersAsync(identity, rule, context.RequestAborted);

                headers.Context = context;

                context.Response.OnStarting(SetRateLimitHeaders, state: headers);
            }

            await _next.Invoke(context);
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
            var message = string.Format(_options.QuotaExceededMessage ?? "API calls quota exceeded! maximum admitted {0} per {1}.", rule.Limit, rule.Period);

            if (!_options.DisableRateLimitHeaders)
            {
                httpContext.Response.Headers["Retry-After"] = retryAfter;
            }

            httpContext.Response.StatusCode = _options.HttpStatusCode;

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