namespace Infrastructure.Persistence.Seeding;

public sealed record RolePermissionSeedData(
    AppDbContext DbContext,
    Guid RoleId,
    IEnumerable<Guid> PermissionIds);
