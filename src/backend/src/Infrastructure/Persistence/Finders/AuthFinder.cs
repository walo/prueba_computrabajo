using Application.Common.Interfaces;
using Application.Features.Auth.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Finders;

public sealed class AuthFinder(AppDbContext dbContext) : IAuthFinder
{
    public async Task<UserIdentityDto?> GetUserIdentityByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var query = from user in dbContext.Users.AsNoTracking()
                    join role in dbContext.Roles.AsNoTracking() on user.RoleId equals role.Id
                    where user.Id == userId
                    select new UserIdentityDto(user.Id, user.Email, user.FullName, role.Code, user.IsActive);

        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<string>> GetPermissionNamesByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var query = from user in dbContext.Users.AsNoTracking()
                    join rolePermission in dbContext.RolePermissions.AsNoTracking() on user.RoleId equals rolePermission.RoleId
                    join permission in dbContext.Permissions.AsNoTracking() on rolePermission.PermissionId equals permission.Id
                    where user.Id == userId && user.IsActive
                    select permission.Code;

        return await query.Distinct().OrderBy(code => code).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Guid>> GetUserIdsByRoleIdAsync(Guid roleId, CancellationToken ct = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Where(user => user.RoleId == roleId)
            .Select(user => user.Id)
            .ToListAsync(ct);
    }
}
