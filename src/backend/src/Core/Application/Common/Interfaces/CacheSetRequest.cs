namespace Application.Common.Interfaces;

public sealed record CacheSetRequest<T>(
    string Key,
    T Value,
    TimeSpan? TimeToLive = null);
