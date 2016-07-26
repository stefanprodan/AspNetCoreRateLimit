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
    public class IpRateLimitController : Controller
    {
        private readonly IpRateLimitOptions _options;
        private readonly IIpPolicyStore _ipPolicyStore;

        public IpRateLimitController(IOptions<IpRateLimitOptions> optionsAccessor, IIpPolicyStore ipPolicyStore)
        {
            _options = optionsAccessor.Value;
            _ipPolicyStore = ipPolicyStore;
        }

        [HttpGet]
        public IpRateLimitPolicies Get()
        {
            return null;
        }

        [HttpPost]
        public void Post()
        {

        }
    }
}
