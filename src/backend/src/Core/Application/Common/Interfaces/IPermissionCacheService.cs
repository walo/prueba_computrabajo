namespace Application.Common.Interfaces;

public interface IPermissionCacheService
{
    Task<HashSet<string>> GetPermissionsForUserAsync(Guid userId, CancellationToken ct = default);
    Task InvalidateUserPermissionsAsync(Guid userId, CancellationToken ct = default);
    Task InvalidateRolePermissionsAsync(Guid roleId, CancellationToken ct = default);
}
