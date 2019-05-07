using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AspNetCoreRateLimit.Demo.Controllers
{
    [Route("api/[controller]")]
    public class MPRateLimitController : Controller
    {
        private readonly MPRateLimitOptions _options;
        private readonly IMPPolicyStore _mpPolicyStore;

        public MPRateLimitController(IOptions<MPRateLimitOptions> optionsAccessor, IMPPolicyStore mpPolicyStore)
        {
            _options = optionsAccessor.Value;
            _mpPolicyStore = mpPolicyStore;
        }

        [HttpGet]
        public async Task<MPRateLimitPolicy> Get()
        {
            return await _mpPolicyStore.GetAsync($"{_options.MPRatePolicyPrefix}_cl-key-1", HttpContext.RequestAborted);
        }

        [HttpPost]
        public async Task Post()
        {
            var id = $"{_options.MPRatePolicyPrefix}_cl-key-1";
            var policy = await _mpPolicyStore.GetAsync(id, HttpContext.RequestAborted);

            policy.Rules.Add(new RateLimitRule
            {
                Endpoint = "*/api/testpolicyupdate",
                Period = "1h",
                Limit = 100
            });

            await _mpPolicyStore.SetAsync(id, policy, cancellationToken: HttpContext.RequestAborted);
        }
    }
}
