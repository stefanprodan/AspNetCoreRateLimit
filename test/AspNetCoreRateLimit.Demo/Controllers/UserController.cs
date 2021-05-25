using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreRateLimit.Demo.Controllers
{
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        // GET api/user/get-all
        [Route("get-all")]
        [HttpGet]
        public GetAllUserOutput GetAll()
        {
            var output = new GetAllUserOutput();
            output.Data = new List<string>()
            {
                "Tom",
                "Jack"
            };
            return output;
        }

        // GET api/user/get-info
        [Route("get-info")]
        [HttpGet]
        public GeUserInfoOutput GetUserInfo()
        {
            var output = new GeUserInfoOutput();
            output.Name = "Tom";
            return output;
        }
    }

    public class GetAllUserOutput
    {
        public List<string> Data { get; set; }

        public string Error { get; set; }
    }

    public class GeUserInfoOutput
    {
        public string Name { get; set; }

        public string Error { get; set; }

    }
}