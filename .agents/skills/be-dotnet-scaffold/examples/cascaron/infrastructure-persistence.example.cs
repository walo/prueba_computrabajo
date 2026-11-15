// NOTA: Generar 1 archivo físico por clase dentro de src/Infrastructure/Persistence/:
// - AppDbContext.cs
// - Configurations/{UserConfiguration.cs, RoleConfiguration.cs, PermissionConfiguration.cs, RefreshTokenConfiguration.cs}
// - Repositories/{UserRepository.cs, RefreshTokenRepository.cs}
// - Finders/{AuthFinder.cs, UserFinder.cs}
// - Seeding/InitialDataSeeder.cs

using Domain.Entities;
using Domain.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

namespace Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(rt => rt.Id);
        // OBLIGATORIO en IDs Guid generados en cliente para evitar DbUpdateConcurrencyException
        builder.Property(rt => rt.Id).ValueGeneratedNever();

        builder.Property(rt => rt.TokenHash).IsRequired().HasMaxLength(256);
        builder.Property(rt => rt.ReplacedByTokenHash).HasMaxLength(256);
        builder.HasIndex(rt => rt.TokenHash).IsUnique();
        builder.HasIndex(rt => rt.UserId);
    }
}
