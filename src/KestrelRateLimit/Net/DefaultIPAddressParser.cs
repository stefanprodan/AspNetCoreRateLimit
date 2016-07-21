using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace KestrelRateLimit
{
    public class DefaultIpAddressParser : IIPAddressParser
    {
        public bool ContainsIp(List<string> ipRules, string clientIp)
        {
            return IPAddressUtil.ContainsIp(ipRules, clientIp);
        }

        public bool ContainsIp(List<string> ipRules, string clientIp, out string rule)
        {
            return IPAddressUtil.ContainsIp(ipRules, clientIp, out rule);
        }

        public virtual IPAddress GetClientIp(HttpContext context)
        {
            return context.Connection.RemoteIpAddress;
        }

        public IPAddress ParseIp(string ipAddress)
        {
            return IPAddressUtil.ParseIp(ipAddress);
        }
    }
}
