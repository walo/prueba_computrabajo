using Application.Features.Auth.DTOs;

namespace Application.Common.Interfaces;

public interface IAuthFinder
{
    Task<UserIdentityDto?> GetUserIdentityByIdAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetPermissionNamesByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> GetUserIdsByRoleIdAsync(Guid roleId, CancellationToken ct = default);
}
