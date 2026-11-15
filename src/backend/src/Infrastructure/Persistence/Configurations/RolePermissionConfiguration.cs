using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");
        builder.HasKey(rolePermission => new { rolePermission.RoleId, rolePermission.PermissionId });
        builder.Property(rolePermission => rolePermission.RoleId).IsRequired();
        builder.Property(rolePermission => rolePermission.PermissionId).IsRequired();
        builder.HasIndex(rolePermission => rolePermission.PermissionId);
    }
}
