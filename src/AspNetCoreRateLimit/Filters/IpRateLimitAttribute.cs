using System;

namespace AspNetCoreRateLimit
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class IpRateLimitAttribute : RateLimitAttribute
    {
    }
}