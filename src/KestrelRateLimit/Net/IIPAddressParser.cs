using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace KestrelRateLimit
{
    public interface IIPAddressParser
    {
        bool ContainsIp(List<string> ipRules, string clientIp);

        bool ContainsIp(List<string> ipRules, string clientIp, out string rule);

        IPAddress GetClientIp(HttpContext context);

        IPAddress ParseIp(string ipAddress);
    }
}
