using Application.Common.Interfaces;
using Application.Features.Users.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Finders;

public sealed class UserFinder(AppDbContext dbContext) : IUserFinder
{
    public async Task<IReadOnlyList<UserSummaryResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var query = from user in dbContext.Users.AsNoTracking()
                    join role in dbContext.Roles.AsNoTracking() on user.RoleId equals role.Id
                    orderby user.CreatedAtUtc descending
                    select new UserSummaryResponse(
                        user.Id,
                        user.FullName,
                        user.Email,
                        role.Name,
                        user.IsActive,
                        user.CreatedAtUtc);

        return await query.ToListAsync(ct);
    }
}
