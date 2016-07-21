using System;

namespace KestrelRateLimit
{
    public interface IRateLimitStore
    {
        void ClearCounters();
        bool CounterExists(string id);
        RateLimitCounter? GetCounter(string id);
        void RemoveCounter(string id);
        void SaveCounter(string id, RateLimitCounter counter, TimeSpan expirationTime);
    }
}