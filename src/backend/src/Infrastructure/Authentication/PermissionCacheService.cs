using Application.Common.Interfaces;

namespace Infrastructure.Authentication;

public sealed class PermissionCacheService(
    ICacheService cacheService,
    IAuthFinder authFinder) : IPermissionCacheService
{
    private static readonly TimeSpan DefaultTimeToLive = TimeSpan.FromMinutes(60);

    public async Task<HashSet<string>> GetPermissionsForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var cacheKey = BuildUserCacheKey(userId);
        var cachedPermissions = await cacheService.GetAsync<List<string>>(cacheKey, ct);
        if (cachedPermissions is not null)
        {
            return cachedPermissions.ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
        var permissions = await authFinder.GetPermissionNamesByUserIdAsync(userId, ct);
        await cacheService.SetAsync(new CacheSetRequest<List<string>>(cacheKey, permissions.ToList(), DefaultTimeToLive), ct);

        return permissions.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public Task InvalidateUserPermissionsAsync(Guid userId, CancellationToken ct = default)
    {
        return cacheService.RemoveAsync(BuildUserCacheKey(userId), ct);
    }

    public async Task InvalidateRolePermissionsAsync(Guid roleId, CancellationToken ct = default)
    {
        var userIds = await authFinder.GetUserIdsByRoleIdAsync(roleId, ct);
        foreach (var userId in userIds)
        {
            await InvalidateUserPermissionsAsync(userId, ct);
        }
    }

    private static string BuildUserCacheKey(Guid userId)
    {
        return $"permissions:user:{userId}";
    }
}
