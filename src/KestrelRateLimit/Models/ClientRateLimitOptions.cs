using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KestrelRateLimit
{
    public class ClientRateLimitOptions : RateLimitCoreOptions
    {
        /// <summary>
        /// Gets or sets the HTTP header that holds the client identifier, by default is X-ClientId
        /// </summary>
        public string ClientIdHeader { get; set; } = "X-ClientId";       
    }
}
