using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace AspNetCoreRateLimit.Demo.Controllers
{
    [Route("api/[controller]")]
    public class MPValuesController : Controller
    {
        // GET api/mpvalues
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/mpvalues/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/mpvalues
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/mpvalues/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
