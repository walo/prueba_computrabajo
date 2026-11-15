using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Seeding;

public static class InitialDataSeeder
{
    public static async Task SeedAsync(DatabaseSeedRequest request, CancellationToken ct = default)
    {
        await EnsureDatabaseSchemaAsync(request.DbContext, ct);
        await SeedPermissionsAsync(request.DbContext, ct);
        await SeedRolesAsync(request.DbContext, ct);
        await SeedRolePermissionsAsync(request.DbContext, ct);
        await SeedDefaultUsersAsync(request, ct);
        request.Logger.LogInformation("Database seeding verified successfully.");
    }

    private static async Task EnsureDatabaseSchemaAsync(AppDbContext dbContext, CancellationToken ct)
    {
        if (dbContext.Database.IsRelational())
        {
            await dbContext.Database.MigrateAsync(ct);

            return;
        }
        await dbContext.Database.EnsureCreatedAsync(ct);
    }

    private static async Task SeedPermissionsAsync(AppDbContext dbContext, CancellationToken ct)
    {
        var existingCodes = await dbContext.Permissions.Select(p => p.Code).ToListAsync(ct);
        var existingSet = existingCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missing = Permissions.GetAll()
            .Where(item => !existingSet.Contains(item.Code))
            .Select(Permission.Create)
            .ToList();
        if (missing.Count == 0)
        {
            return;
        }
        await dbContext.Permissions.AddRangeAsync(missing, ct);
        await dbContext.SaveChangesAsync(ct);
    }

    private static async Task SeedRolesAsync(AppDbContext dbContext, CancellationToken ct)
    {
        var existingCodes = await dbContext.Roles.Select(r => r.Code).ToListAsync(ct);
        var existingSet = existingCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var requiredRoles = new[]
        {
            new RoleCreationData("ADMIN", "Administrator", "Full administrative access"),
            new RoleCreationData("USER", "Standard User", "Read-only inventory access")
        };
        var missing = requiredRoles
            .Where(item => !existingSet.Contains(item.Code))
            .Select(Role.Create)
            .ToList();
        if (missing.Count == 0)
        {
            return;
        }
        await dbContext.Roles.AddRangeAsync(missing, ct);
        await dbContext.SaveChangesAsync(ct);
    }

    private static async Task SeedRolePermissionsAsync(AppDbContext dbContext, CancellationToken ct)
    {
        var permissions = await dbContext.Permissions.AsNoTracking().ToListAsync(ct);
        var adminRole = await dbContext.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Code == "ADMIN", ct);
        var userRole = await dbContext.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Code == "USER", ct);
        if (adminRole is not null)
        {
            await AssignMissingPermissionsAsync(dbContext, adminRole.Id, permissions.Select(p => p.Id), ct);
        }
        if (userRole is not null)
        {
            var userCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                Permissions.Categories.Read,
                Permissions.Products.Read,
                Permissions.InventoryMovements.Read
            };
            var userPermissionIds = permissions.Where(p => userCodes.Contains(p.Code)).Select(p => p.Id);
            await AssignMissingPermissionsAsync(dbContext, userRole.Id, userPermissionIds, ct);
        }
    }

    private static async Task AssignMissingPermissionsAsync(
        AppDbContext dbContext,
        Guid roleId,
        IEnumerable<Guid> permissionIds,
        CancellationToken ct)
    {
        var existingIds = await dbContext.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.PermissionId)
            .ToListAsync(ct);
        var existingSet = existingIds.ToHashSet();
        var missing = permissionIds
            .Where(id => !existingSet.Contains(id))
            .Select(id => RolePermission.Create(new RolePermissionCreationData(roleId, id)))
            .ToList();
        if (missing.Count == 0)
        {
            return;
        }
        await dbContext.RolePermissions.AddRangeAsync(missing, ct);
        await dbContext.SaveChangesAsync(ct);
    }

    private static async Task SeedDefaultUsersAsync(DatabaseSeedRequest request, CancellationToken ct)
    {
        var dbContext = request.DbContext;
        var adminRole = await dbContext.Roles.AsNoTracking().FirstAsync(r => r.Code == "ADMIN", ct);
        var userRole = await dbContext.Roles.AsNoTracking().FirstAsync(r => r.Code == "USER", ct);
        await EnsureUserExistsAsync(request, new UserCreationData("System Administrator", "admin@enterprise.local", request.PasswordHasher.Hash("Admin123*"), adminRole.Id), ct);
        await EnsureUserExistsAsync(request, new UserCreationData("Standard Operator", "user@enterprise.local", request.PasswordHasher.Hash("User123*"), userRole.Id), ct);
    }

    private static async Task EnsureUserExistsAsync(DatabaseSeedRequest request, UserCreationData data, CancellationToken ct)
    {
        var exists = await request.DbContext.Users.AnyAsync(u => u.Email == data.Email, ct);
        if (exists)
        {
            return;
        }
        await request.DbContext.Users.AddAsync(User.Create(data), ct);
        await request.DbContext.SaveChangesAsync(ct);
    }
}
