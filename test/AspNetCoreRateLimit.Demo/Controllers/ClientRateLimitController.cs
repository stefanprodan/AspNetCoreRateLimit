using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit.Demo.Controllers
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
        public async Task<ClientRateLimitPolicy> Get()
        {
            return await _clientPolicyStore.GetAsync($"{_options.ClientPolicyPrefix}_cl-key-1", HttpContext.RequestAborted);
        }

        [HttpPost]
        public async Task Post()
        {
            var id = $"{_options.ClientPolicyPrefix}_cl-key-1";
            var policy = await _clientPolicyStore.GetAsync(id, HttpContext.RequestAborted);

            policy.Rules.Add(new RateLimitRule
            {
                Endpoint = "*/api/testpolicyupdate",
                Period = "1h",
                Limit = 100
            });

            await _clientPolicyStore.SetAsync(id, policy, cancellationToken: HttpContext.RequestAborted);
        }
    }
}