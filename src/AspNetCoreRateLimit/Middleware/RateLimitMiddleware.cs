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

        protected RateLimitMiddleware(
            RequestDelegate next,
            RateLimitOptions options,
            TProcessor processor)
        {
            _next = next;
            _options = options;
            _processor = processor;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            // check if rate limiting is enabled
            if (_options == null)
            {
                await _next.Invoke(httpContext);
                return;
            }

            // compute identity from request
            var identity = SetIdentity(httpContext);

            // check white list
            if (_processor.IsWhitelisted(identity))
            {
                await _next.Invoke(httpContext);
                return;
            }

            var rules = _processor.GetMatchingRules(identity);

            foreach (var rule in rules)
            {
                if (rule.Limit > 0)
                {
                    // increment counter
                    var counter = _processor.ProcessRequest(identity, rule);

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
                        LogBlockedRequest(httpContext, identity, counter, rule);

                        // break execution
                        await ReturnQuotaExceededResponse(httpContext, rule, retryAfter);
                        return;
                    }
                }
                // if limit is zero or less, block the request.
                else
                {
                    // process request count
                    var counter = _processor.ProcessRequest(identity, rule);

                    // log blocked request
                    LogBlockedRequest(httpContext, identity, counter, rule);

                    // break execution (Int32 max used to represent infinity)
                    await ReturnQuotaExceededResponse(httpContext, rule, int.MaxValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    return;
                }
            }

            //set X-Rate-Limit headers for the longest period
            if (rules.Any() && !_options.DisableRateLimitHeaders)
            {
                var rule = rules.OrderByDescending(x => x.PeriodTimespan.Value).First();
                var headers = _processor.GetRateLimitHeaders(identity, rule);
                headers.Context = httpContext;

                httpContext.Response.OnStarting(SetRateLimitHeaders, state: headers);
            }

            await _next.Invoke(httpContext);
        }

        public virtual ClientRequestIdentity SetIdentity(HttpContext httpContext)
        {
            //var clientId = "anon";
            //if (httpContext.Request.Headers.Keys.Contains(_options.ClientIdHeader, StringComparer.CurrentCultureIgnoreCase))
            //{
            //    clientId = httpContext.Request.Headers[_options.ClientIdHeader].First();
            //}

            //string clientIp;
            //try
            //{
            //    var ip = _ipParser.GetClientIp(httpContext);

            //    if (ip == null)
            //    {
            //        throw new Exception("IpRateLimitMiddleware can't parse caller IP");
            //    }

            //    clientIp = ip.ToString();
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception("IpRateLimitMiddleware can't parse caller IP", ex);
            //}

            return new ClientRequestIdentity
            {
                //ClientIp = clientIp,
                Path = httpContext.Request.Path.ToString().ToLowerInvariant(),
                HttpVerb = httpContext.Request.Method.ToLowerInvariant(),
                //ClientId = clientId
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