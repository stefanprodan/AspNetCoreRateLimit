using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace KestrelRateLimit.Demo.Controllers
{
    [Route("api/[controller]")]
    public class RateLimitController : Controller
    {
        private readonly MemoryCacheRateLimitStore _rateLimitStore;
        private readonly RateLimitOptions _optionsFromAppsettings;

        public RateLimitController(IOptions<RateLimitOptions> optionsAccessor, IMemoryCache memoryCache)
        {
            _optionsFromAppsettings = optionsAccessor.Value;
            _rateLimitStore = new MemoryCacheRateLimitStore(memoryCache);
        }

        [HttpGet]
        public RateLimitOptions Get()
        {
            return _rateLimitStore.GetOptions(_optionsFromAppsettings.GetOptionsKey());
        }

        [HttpPost]
        public void Post()
        {
            var opt = _rateLimitStore.GetOptions(_optionsFromAppsettings.GetOptionsKey());
            opt.EndpointWhitelist.Add("get:/api/testupdate");
            _rateLimitStore.SaveOptions(opt.GetOptionsKey(), opt);
        }
    }
}
