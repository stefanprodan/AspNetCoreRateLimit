using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KestrelRateLimit
{
    public class RateLimitHeaders
    {
        public HttpContext Context { get; set; }

        public string Limit { get; set; }

        public string Remaining { get; set; }

        public string Reset { get; set; }
    }
}
