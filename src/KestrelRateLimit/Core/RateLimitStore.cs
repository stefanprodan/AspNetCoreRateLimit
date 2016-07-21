using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KestrelRateLimit
{
    public class RateLimitStore: IRateLimitStore
    {
        public void SaveCounter(string id, RateLimitCounter counter, TimeSpan expirationTime)
        {

        }

        public bool CounterExists(string id)
        {
            return true;
        }

        public RateLimitCounter? GetCounter(string id)
        {
            return null;
        }

        public void RemoveCounter(string id)
        {

        }

        public void ClearCounters()
        {

        }
    }
}
