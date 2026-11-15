// NOTA: Ubicado en src/Infrastructure/Shared/Caching/CacheService.cs (Librería: Infrastructure.Shared)
// Implementa Redis primario con fallback automático a IMemoryCache si Redis está desconectado o en hosts de prueba.

using System.Text.Json;
using Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;

namespace Infrastructure.Shared.Caching;

public sealed class CacheService(
    IMemoryCache memoryCache,
    IConnectionMultiplexer? redis = null) : ICacheService
{
    private readonly IDatabase? _db = redis?.IsConnected == true ? redis.GetDatabase() : null;

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        if (_db is not null)
        {
            var value = await _db.StringGetAsync(key);
            return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(value.ToString());
        }

        return memoryCache.TryGetValue(key, out T? cached) ? cached : default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = default, CancellationToken ct = default)
    {
        if (_db is not null)
        {
            var json = JsonSerializer.Serialize(value);
            await _db.StringSetAsync(key, json, ttl.HasValue ? (Expiration)ttl.Value : default);
            return;
        }

        if (ttl.HasValue)
            memoryCache.Set(key, value, ttl.Value);
        else
            memoryCache.Set(key, value);
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        if (_db is not null)
        {
            await _db.KeyDeleteAsync(key);
            return;
        }

        memoryCache.Remove(key);
    }
}
