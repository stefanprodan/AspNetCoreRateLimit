using System;
using System.Globalization;

namespace AspNetCoreRateLimit
{
    public static class Extensions
    {
        public static bool ContainsIgnoreCase(this string source, string value, StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
        {
            return source != null && value != null && source.IndexOf(value, stringComparison) >= 0;
        }

        public static string RetryAfterFrom(this DateTime timestamp, RateLimitRule rule)
        {
            var secondsPast = Convert.ToInt32((DateTime.UtcNow - timestamp).TotalSeconds);
            var retryAfter = Convert.ToInt32(rule.PeriodTimespan.Value.TotalSeconds);
            retryAfter = retryAfter > 1 ? retryAfter - secondsPast : 1;
            return retryAfter.ToString(CultureInfo.InvariantCulture);
        }

        public static TimeSpan ToTimeSpan(this string timeSpan)
        {
            var l = timeSpan.Length - 1;
            var value = timeSpan.Substring(0, l);
            var type = timeSpan.Substring(l, 1);

            switch (type)
            {
                case "d": return TimeSpan.FromDays(double.Parse(value));
                case "h": return TimeSpan.FromHours(double.Parse(value));
                case "m": return TimeSpan.FromMinutes(double.Parse(value));
                case "s": return TimeSpan.FromSeconds(double.Parse(value));
                default: throw new FormatException($"{timeSpan} can't be converted to TimeSpan, unknown type {type}");
            }
        }
    }
}