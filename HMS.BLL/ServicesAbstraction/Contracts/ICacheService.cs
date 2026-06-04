namespace HMS.BLL.ServicesAbstraction.Contracts
{
    public interface ICacheService
    {
        Task<string?> GetCachedValueAsync(string key);
        Task SetCacheValueAsync(string key, object value, TimeSpan duration);
        Task RemoveAsync(string key);
    }
}
