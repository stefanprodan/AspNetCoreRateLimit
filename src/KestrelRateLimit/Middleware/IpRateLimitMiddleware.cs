using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace KestrelRateLimit
{
    public class IpRateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly IIpAddressParser _ipParser;
        private readonly IMemoryCache _memoryCache;
        private readonly IpRateLimitProcessor _processor;

        private RateLimitOptions _options;

        public IpRateLimitMiddleware(RequestDelegate next, 
            IOptions<RateLimitOptions> options, 
            ILoggerFactory loggerFactory,
            IMemoryCache memoryCache = null,
            IIpAddressParser ipParser = null
            )
        {
            _next = next;
            _options = options.Value;
            _logger = loggerFactory.CreateLogger<IpRateLimitMiddleware>();
            _ipParser = ipParser != null ? ipParser : new ReversProxyIpParser(_options.RealIpHeader);
            _memoryCache = memoryCache;

            _processor = new IpRateLimitProcessor(_options, new MemoryCacheRateLimitStore(_memoryCache), _ipParser);
            _options = _processor.GetSetOptionsInCache();
        }

        public async Task Invoke(HttpContext context)
        {
            //_logger.LogInformation($"Rate limiting request {context.Request.Method.ToLowerInvariant()}:{context.Request.Path.ToString().ToUpperInvariant()} id {context.TraceIdentifier}");

            if (context.Request.Headers.Keys.Contains("X-Rate-Limit-Info"))
            {
                context.Response.OnStarting(SetRateLimitHeaders, state: new RateLimitHeaders { Context = context, Limit = "per day", Remaining = "200", Reset = "2016.07.21 12:30:59" });
            }

            // check if rate limiting is enabled
                if (_options == null)
            {
                await _next.Invoke(context);
                return;
            }

            // compute identity from request
            var identity = SetIdentity(context);

            // check white list
            if (_processor.IsWhitelisted(identity))
            {
                await _next.Invoke(context);
                return;
            }

            var timeSpan = TimeSpan.FromSeconds(1);

            // get default rates
            var defRates = _processor.RatesWithDefaults(_options.ComputeRates().ToList());
            if (_options.StackBlockedRequests)
            {
                // all requests including the rejected ones will stack in this order: week, day, hour, min, sec
                // if a client hits the hour limit then the minutes and seconds counters will expire and will eventually get erased from cache
                defRates.Reverse();
            }

            // apply policy
            foreach (var rate in defRates)
            {
                var rateLimitPeriod = rate.Key;
                var rateLimit = rate.Value;

                timeSpan = _processor.GetTimeSpanFromPeriod(rateLimitPeriod);

                // apply global rules
                _processor.ApplyRules(identity, timeSpan, rateLimitPeriod, ref rateLimit);

                if (rateLimit > 0)
                {
                    // increment counter
                    var counterData = _processor.ProcessRequest(identity, timeSpan, rateLimitPeriod);

                    // check if key expired
                    if (counterData.Counter.Timestamp + timeSpan < DateTime.UtcNow)
                    {
                        continue;
                    }

                    // check if limit is reached
                    if (counterData.Counter.TotalRequests > rateLimit)
                    {
                        //compute retry after value
                        var retryAfter = _processor.RetryAfterFrom(counterData.Counter.Timestamp, rateLimitPeriod);

                        // log blocked request
                        _logger.LogInformation($"Request {identity.HttpVerb}:{identity.Endpoint} from IP {identity.ClientIp} ClienId {identity.ClientBypassKey} has been blocked, quota {rateLimit}/{rateLimitPeriod.ToString()} exceeded by {counterData.Counter.TotalRequests}. Rule {counterData.Key}");

                        var message = string.IsNullOrEmpty(_options.QuotaExceededMessage) ? $"API calls quota exceeded! maximum admitted {rateLimit} per {rateLimitPeriod.ToString()}. Rule {counterData.Key}" : _options.QuotaExceededMessage;

                        // break execution
                        await QuotaExceededResponse(context, _options.HttpStatusCode, message, retryAfter);
                        return;
                    }
                }
            }

            await _next.Invoke(context);

            //_logger.LogInformation($"Finished handling request {context.TraceIdentifier}");
        }

        public virtual RequestIdentity SetIdentity(HttpContext httpContext)
        {
            var clientId = "anon";
            if (httpContext.Request.Headers.Keys.Contains(_options.BypassHeader))
            {
                clientId = httpContext.Request.Headers[_options.BypassHeader].First();
            }

            var clientIp = string.Empty;
            try
            {
                var ip = _ipParser.GetClientIp(httpContext);
                if(ip == null)
                {
                    throw new Exception("IpRateLimitMiddleware can't parse caller IP");
                }

                clientIp = ip.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("IpRateLimitMiddleware can't parse caller IP", ex);
            }

            return new RequestIdentity
            {
                ClientIp = clientIp,
                Endpoint = httpContext.Request.Path.ToString().ToLowerInvariant(),
                HttpVerb = httpContext.Request.Method.ToLowerInvariant(),
                ClientBypassKey = clientId
            };
        }

        public virtual Task QuotaExceededResponse(HttpContext httpContext, int statusCode, string message, string retryAfter)
        {
            httpContext.Response.Headers["Retry-After"] = retryAfter;
            httpContext.Response.StatusCode = statusCode;
            return httpContext.Response.WriteAsync(message);
        }

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
