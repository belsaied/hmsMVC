using HMS.DAL.Contracts;
using Services.Abstraction.Contracts;

namespace HMS.BLL.Services.Implementations
{
    public class CacheService (ICacheRepository _cacheRepository) : ICacheService
    {
        public async Task<string?> GetCachedValueAsync(string key)
            => await _cacheRepository.GetAsync(key);
        public async Task SetCacheValueAsync(string key, object value, TimeSpan duration)
            => await _cacheRepository.SetAsync(key, value, duration);
        public async Task RemoveAsync(string key)
            => await _cacheRepository.RemoveAsync(key);
    }
}
