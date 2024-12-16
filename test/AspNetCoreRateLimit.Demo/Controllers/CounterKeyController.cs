using AspNetCoreRateLimit.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AspNetCoreRateLimit.Demo.Controllers
{
    [Route("api/[controller]")]
    public class CounterKeyController : Controller
    {
        [HttpGet, HttpPost, HttpOptions, HttpDelete, HttpPut, HttpPatch]
        [Route("VerbAndPath")]
        public string VerbAndPath()
        {
            return Request.Method;
        }

        [HttpGet, HttpPost, HttpOptions, HttpDelete, HttpPut, HttpPatch]
        [Route("Path")]
        public string Path()
        {
            return Request.Method;
        }

        [HttpGet, HttpPost, HttpOptions, HttpDelete, HttpPut, HttpPatch]
        [Route("VerbAndRule/{id:int}")]
        public string VerbAndRule(int id)
        {
            return $"{Request.Method}_{id}";
        }

        [HttpGet, HttpPost, HttpOptions, HttpDelete, HttpPut, HttpPatch]
        [Route("Rule/{id:int}")]
        public string Rule(int id)
        {
            return $"{Request.Method}_{id}";
        }

        [HttpGet]
        [Route("TestCustomQuotaExceededResponse")]
        public string TestCustomQuotaExceededResponse()
        {
            return Request.Method;
        }

        [HttpGet]
        [Route("CustomQuotaExceededResponse")]
        public string CustomQuotaExceededResponse(QuotaExceededParams quotaExceededParams)
        {
            Response.StatusCode = 429;
            return $"This is customQuotaExceededResponse.\r\n{(quotaExceededParams == null ? string.Empty:JsonSerializer.Serialize(quotaExceededParams))}";
        }
    }
}
