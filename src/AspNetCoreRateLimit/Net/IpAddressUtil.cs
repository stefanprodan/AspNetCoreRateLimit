using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace AspNetCoreRateLimit
{
    public class IpAddressUtil
    {
        public static bool ContainsIp(string rule, string clientIp)
        {
            var ip = ParseIp(clientIp);

            var range = new IpAddressRange(rule);

            if (range.Contains(ip))
            {
                return true;
            }

            return false;
        }

        public static bool ContainsIp(List<string> ipRules, string clientIp)
        {
            if (ipRules?.Any() == true)
            {
                var ip = ParseIp(clientIp);

                foreach (var rule in ipRules)
                {
                    var range = new IpAddressRange(rule);

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

            if (ipRules?.Any() == true)
            {
                var ip = ParseIp(clientIp);

                foreach (var r in ipRules)
                {
                    var range = new IpAddressRange(r);

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
            //remove port number from ip address if any
            ipAddress = ipAddress.Split(',').First().Trim();

            var portDelimiterPos = ipAddress.LastIndexOf(":", StringComparison.CurrentCultureIgnoreCase);
            var ipv6WithPortStart = ipAddress.StartsWith("[");
            var ipv6End = ipAddress.IndexOf("]");

            if (portDelimiterPos != -1
                && portDelimiterPos == ipAddress.IndexOf(":", StringComparison.CurrentCultureIgnoreCase)
                || ipv6WithPortStart && ipv6End != -1 && ipv6End < portDelimiterPos)
            {
                ipAddress = ipAddress.Substring(0, portDelimiterPos);
            }

            return IPAddress.Parse(ipAddress);
        }
    }
}