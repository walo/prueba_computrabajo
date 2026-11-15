using System.Text.Json;
using Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;

namespace Infrastructure.Shared.Caching;

public sealed class CacheService(
    IMemoryCache memoryCache,
    IConnectionMultiplexer? redis = null) : ICacheService
{
    private readonly IDatabase? _database = redis?.IsConnected == true ? redis.GetDatabase() : null;

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        if (_database is not null)
        {
            var value = await _database.StringGetAsync(key);

            return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(value.ToString());
        }

        return memoryCache.TryGetValue(key, out T? cached) ? cached : default;
    }

    public async Task SetAsync<T>(CacheSetRequest<T> request, CancellationToken ct = default)
    {
        if (_database is not null)
        {
            await SetRedisValueAsync(request);

            return;
        }
        if (request.TimeToLive.HasValue)
        {
            memoryCache.Set(request.Key, request.Value, request.TimeToLive.Value);

            return;
        }
        memoryCache.Set(request.Key, request.Value);
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        if (_database is not null)
        {
            await _database.KeyDeleteAsync(key);

            return;
        }
        memoryCache.Remove(key);
    }

    private Task SetRedisValueAsync<T>(CacheSetRequest<T> request)
    {
        var payload = JsonSerializer.Serialize(request.Value);

        return _database!.StringSetAsync(request.Key, payload, request.TimeToLive);
    }
}
