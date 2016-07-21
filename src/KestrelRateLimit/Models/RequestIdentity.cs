using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KestrelRateLimit
{
    /// <summary>
    /// Stores the client IP, key and endpoint
    /// </summary>
    public class RequestIdentity
    {
        public string ClientIp { get; set; }

        public string ClientKey { get; set; }

        public string Endpoint { get; set; }
    }
}
