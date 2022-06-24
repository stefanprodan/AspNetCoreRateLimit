namespace AspNetCoreRateLimit.Redis.BodyParameter.Store
{
    public interface IBodyParameterRateLimitStore<T>
    {
        bool Exists(string id);
        Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);
        T Get(string id);
        Task<T> GetAsync(string id, CancellationToken cancellationToken = default);
        void Remove(string id);
        Task RemoveAsync(string id, CancellationToken cancellationToken = default);
        void Set(string id, T entry, TimeSpan? expirationTime = null);
        Task SetAsync(string id, T entry, TimeSpan? expirationTime = null, CancellationToken cancellationToken = default);
    }
}