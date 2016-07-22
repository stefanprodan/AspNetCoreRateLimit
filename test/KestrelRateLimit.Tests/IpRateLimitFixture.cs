using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KestrelRateLimit.Tests
{
    public class IpRateLimitFixture<TStartup> : IpRateLimitFixtureBase<TStartup>
        where TStartup : class
    {
        public IpRateLimitFixture() : base("http://localhost:5000")
        {
        }
    }
}
