using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace KestrelRateLimit
{
    public class ReversRoxyIpParser : RemoteIpParser
    {
        public override IPAddress GetClientIp(HttpContext context)
        {
            const string realIpHeader = "X-Real-IP";

            if (context.Request.Headers.Keys.Contains(realIpHeader, StringComparer.CurrentCultureIgnoreCase))
            {
                return ParseIp(context.Request.Headers[realIpHeader].First());
            }

            return base.GetClientIp(context);
        }
    }
}
