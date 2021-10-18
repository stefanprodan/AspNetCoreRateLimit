using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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
            _config.RegisterResolvers();
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
            var identity = await ResolveIdentityAsync(context);

            // check white list
            if (_processor.IsWhitelisted(identity))
            {
                await _next.Invoke(context);
                return;
            }

            var rules = await _processor.GetMatchingRulesAsync(identity, context.RequestAborted);

            var rulesDict = new Dictionary<RateLimitRule, RateLimitCounter>();

            foreach (var rule in rules)
            {
                // increment counter
                var rateLimitCounter = await _processor.ProcessRequestAsync(identity, rule, context.RequestAborted);

                if (rule.Limit > 0)
                {
                    // check if key expired
                    if (rateLimitCounter.Timestamp + rule.PeriodTimespan.Value < DateTime.UtcNow)
                    {
                        continue;
                    }

                    // check if limit is reached
                    if (rateLimitCounter.Count > rule.Limit)
                    {
                        //compute retry after value
                        var retryAfter = rateLimitCounter.Timestamp.RetryAfterFrom(rule);

                        // log blocked request
                        LogBlockedRequest(context, identity, rateLimitCounter, rule);

                        if (_options.RequestBlockedBehaviorAsync != null)
                        {
                            await _options.RequestBlockedBehaviorAsync(context, identity, rateLimitCounter, rule);
                        }

                        if (!rule.MonitorMode)
                        {
                            // break execution
                            await ReturnQuotaExceededResponse(context, rule, retryAfter);

                            return;
                        }
                    }
                }
                // if limit is zero or less, block the request.
                else
                {
                    // log blocked request
                    LogBlockedRequest(context, identity, rateLimitCounter, rule);

                    if (_options.RequestBlockedBehaviorAsync != null)
                    {
                        await _options.RequestBlockedBehaviorAsync(context, identity, rateLimitCounter, rule);
                    }

                    if (!rule.MonitorMode)
                    {
                        // break execution (Int32 max used to represent infinity)
                        await ReturnQuotaExceededResponse(context, rule, int.MaxValue.ToString(System.Globalization.CultureInfo.InvariantCulture));

                        return;
                    }
                }

                rulesDict.Add(rule, rateLimitCounter);
            }

            // set X-Rate-Limit headers for the longest period
            if (rulesDict.Any() && !_options.DisableRateLimitHeaders)
            {
                var rule = rulesDict.OrderByDescending(x => x.Key.PeriodTimespan).FirstOrDefault();
                var headers = _processor.GetRateLimitHeaders(rule.Value, rule.Key, context.RequestAborted);

                headers.Context = context;

                context.Response.OnStarting(SetRateLimitHeaders, state: headers);
            }

            await _next.Invoke(context);
        }

        public virtual async Task<ClientRequestIdentity> ResolveIdentityAsync(HttpContext httpContext)
        {
            string clientIp = null;
            string clientId = null;

            if (_config.ClientResolvers?.Any() == true)
            {
                foreach (var resolver in _config.ClientResolvers)
                {
                    clientId = await resolver.ResolveClientAsync(httpContext);

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
                    clientIp = resolver.ResolveIp(httpContext);

                    if (!string.IsNullOrEmpty(clientIp))
                    {
                        break;
                    }
                }
            }
            var path = httpContext.Request.Path.ToString().ToLowerInvariant();
            return new ClientRequestIdentity
            {
                ClientIp = clientIp,
                Path = path == "/"
                    ? path
                    : path.TrimEnd('/'),
                HttpVerb = httpContext.Request.Method.ToLowerInvariant(),
                ClientId = clientId ?? "anon"
            };
        }

        public virtual Task ReturnQuotaExceededResponse(HttpContext httpContext, RateLimitRule rule, string retryAfter)
        {
            //Use Endpoint QuotaExceededResponse
            if (rule.QuotaExceededResponse != null)
            {
                _options.QuotaExceededResponse = rule.QuotaExceededResponse;
            }
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