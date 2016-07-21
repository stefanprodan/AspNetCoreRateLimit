using System;

namespace KestrelRateLimit
{
    public interface IRateLimitStore
    {
        void RemoveCounter(string id);
        bool CounterExists(string id);
        RateLimitCounter? GetCounter(string id);
        void SaveCounter(string id, RateLimitCounter counter, TimeSpan expirationTime);
        bool OptionsExists(string id);
        void SaveOptions(string id, RateLimitOptions options);
        RateLimitOptions GetOptions(string id);
        void RemoveOptions(string id);
    }
}