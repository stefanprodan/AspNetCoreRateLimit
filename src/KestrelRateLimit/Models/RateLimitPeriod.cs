using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KestrelRateLimit
{
    public enum RateLimitPeriod
    {
        Second = 1,
        Minute,
        Hour,
        Day,
        Week
    }
}
