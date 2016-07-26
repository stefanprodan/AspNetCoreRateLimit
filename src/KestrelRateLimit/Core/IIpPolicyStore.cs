namespace KestrelRateLimit
{
    public interface IIpPolicyStore
    {
        bool Exists(string id);
        IpRateLimitPolicy Get(string id);
        void Remove(string id);
        void Set(string id, IpRateLimitPolicy policy);
    }
}