using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KestrelRateLimit
{
    /// <summary>
    /// Stores the client IP, ID, endpoint and verb
    /// </summary>
    public class ClientRequestIdentity
    {
        public string ClientId { get; set; }

        public string Path { get; set; }

        public string HttpVerb { get; set; }
    }
}
