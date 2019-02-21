using System.Collections.Generic;
using System.Net;

namespace AspNetCoreRateLimit
{
    public static class IpParser
    {
        public static bool ContainsIp(string ipRule, string clientIp)
        {
            return IpAddressUtil.ContainsIp(ipRule, clientIp);
        }

        public static bool ContainsIp(List<string> ipRules, string clientIp)
        {
            return IpAddressUtil.ContainsIp(ipRules, clientIp);
        }

        public static bool ContainsIp(List<string> ipRules, string clientIp, out string rule)
        {
            return IpAddressUtil.ContainsIp(ipRules, clientIp, out rule);
        }

        public static IPAddress ParseIp(string ipAddress)
        {
            return IpAddressUtil.ParseIp(ipAddress);
        }
    }
}