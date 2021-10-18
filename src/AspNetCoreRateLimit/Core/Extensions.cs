using System;
using System.Text.RegularExpressions;

namespace AspNetCoreRateLimit
{
    public static class Extensions
    {
        public static bool IsUrlMatch(this string source, string value, bool useRegex)
        {
            if (useRegex)
            {
                return IsRegexMatch(source, value);
            }
            return source.IsWildCardMatch(value);
        }

        public static bool IsWildCardMatch(this string source, string value)
        {
            return source != null && value != null && source.ToLowerInvariant().IsMatch(value.ToLowerInvariant());
        }

        public static bool IsRegexMatch(this string source, string value)
        {
            if (source == null || string.IsNullOrEmpty(value))
            {
                return false;
            }
            // if the regex is e.g. /api/values/ the path should be an exact match
            // if all paths below this should be included the regex should be /api/values/*
            if (value[value.Length - 1] != '$')
            {
                value += '$';
            }
            if (value[0] != '^')
            {
                value = '^' + value;
            }
            return Regex.IsMatch(source, value, RegexOptions.IgnoreCase);
        }

        public static string RetryAfterFrom(this DateTime timestamp, RateLimitRule rule)
        {
            var diff = timestamp + rule.PeriodTimespan.Value - DateTime.UtcNow;
            var seconds = Math.Max(diff.TotalSeconds, 1);

            return $"{seconds:F0}";
        }

        public static TimeSpan ToTimeSpan(this string timeSpan)
        {
            var l = timeSpan.Length - 1;
            var value = timeSpan.Substring(0, l);
            var type = timeSpan.Substring(l, 1);

            return type switch
            {
                "d" => TimeSpan.FromDays(double.Parse(value)),
                "h" => TimeSpan.FromHours(double.Parse(value)),
                "m" => TimeSpan.FromMinutes(double.Parse(value)),
                "s" => TimeSpan.FromSeconds(double.Parse(value)),
                _ => throw new FormatException($"{timeSpan} can't be converted to TimeSpan, unknown type {type}"),
            };
        }
    }
}