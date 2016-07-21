using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KestrelRateLimit
{
    public class RateLimitEntry
    {
        public string Id { get; set; }
        public string Key { get; set; }
        public RateLimitCounter Counter { get; set; }
    }
}
