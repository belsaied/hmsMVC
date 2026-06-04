using HMS.DAL.Contracts;
using StackExchange.Redis;
using System.Text.Json;

namespace HMS.DAL.Implementations
{
    public class CacheRepository(IConnectionMultiplexer _connectionMultiplexer) : ICacheRepository
    {
        private readonly IDatabase _database = _connectionMultiplexer.GetDatabase();

        public async Task<string?> GetAsync(string key)
        {
            var value = await _database.StringGetAsync(key);
            return value.IsNullOrEmpty ? default : value;
        }

        public async Task SetAsync(string key, object value, TimeSpan duration)
        {
            var serializedObj = JsonSerializer.Serialize(value);
            await _database.StringSetAsync(key, serializedObj, duration);
        }

        public async Task RemoveAsync(string key)
            => await _database.KeyDeleteAsync(key);
    }
}
