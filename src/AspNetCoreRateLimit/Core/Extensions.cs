using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit
{
    public static class Extensions
    {
        public static bool ContainsIgnoreCase(this string source, string value, StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
        {
            return source != null && value != null && source.IndexOf(value, stringComparison) >= 0;
        }
    }
}
