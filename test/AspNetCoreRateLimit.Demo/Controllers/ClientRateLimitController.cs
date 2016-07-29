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
    public class ClientRateLimitController : Controller
    {
        private readonly ClientRateLimitOptions _options;
        private readonly IClientPolicyStore _clientPolicyStore;

        public ClientRateLimitController(IOptions<ClientRateLimitOptions> optionsAccessor, IClientPolicyStore clientPolicyStore)
        {
            _options = optionsAccessor.Value;
            _clientPolicyStore = clientPolicyStore;
        }

        [HttpGet]
        public ClientRateLimitPolicy Get()
        {
            return _clientPolicyStore.Get($"{_options.ClientPolicyPrefix}_cl-key-1");
        }

        [HttpPost]
        public void Post()
        {
            var id = $"{_options.ClientPolicyPrefix}_cl-key-1";
            var anonPolicy = _clientPolicyStore.Get(id);
            anonPolicy.Rules.Add(new RateLimitRule
            {
                Endpoint = "*/api/testpolicyupdate",
                Period = "1h",
                Limit = 100
            });
            _clientPolicyStore.Set(id, anonPolicy);
        }
    }
}
