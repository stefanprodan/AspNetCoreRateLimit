namespace AspNetCoreRateLimit
{
    public interface IPolicyStore<TPolicy>
    {
        bool Exists(string id);
        TPolicy Get(string id);
        void Remove(string id);
        void Set(string id, TPolicy policy);
    }
}