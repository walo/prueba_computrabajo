using Application.Common.Interfaces;

namespace Infrastructure.Authentication;

public sealed class TokenBlacklistService(
    ICacheService cacheService,
    IDateTimeProvider dateTimeProvider) : ITokenBlacklistService
{
    public async Task BlacklistTokenAsync(BlacklistTokenRequest request, CancellationToken ct = default)
    {
        var remainingTime = request.ExpiresAtUtc - dateTimeProvider.UtcNow;
        if (remainingTime <= TimeSpan.Zero)
        {
            return;
        }
        var cacheKey = BuildCacheKey(request.Jti);
        await cacheService.SetAsync(new CacheSetRequest<bool>(cacheKey, true, remainingTime), ct);
    }

    public async Task<bool> IsTokenBlacklistedAsync(string jti, CancellationToken ct = default)
    {
        var cacheKey = BuildCacheKey(jti);

        return await cacheService.GetAsync<bool>(cacheKey, ct);
    }

    private static string BuildCacheKey(string jti)
    {
        return $"blacklist:jti:{jti}";
    }
}
