using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace KestrelRateLimit
{
    public class IPAddressUtil
    {
        public static bool ContainsIp(List<string> ipRules, string clientIp)
        {
            var ip = ParseIp(clientIp);
            if (ipRules != null && ipRules.Any())
            {
                foreach (var rule in ipRules)
                {
                    var range = new IPAddressRange(rule);
                    if (range.Contains(ip))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool ContainsIp(List<string> ipRules, string clientIp, out string rule)
        {
            rule = null;
            var ip = ParseIp(clientIp);
            if (ipRules != null && ipRules.Any())
            {
                foreach (var r in ipRules)
                {
                    var range = new IPAddressRange(r);
                    if (range.Contains(ip))
                    {
                        rule = r;
                        return true;
                    }
                }
            }

            return false;
        }

        public static IPAddress ParseIp(string ipAddress)
        {
            return IPAddress.Parse(ipAddress);
        }
    }
}
