using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");
        builder.HasKey(permission => permission.Id);
        builder.Property(permission => permission.Id).ValueGeneratedNever();
        builder.Ignore(permission => permission.DomainEvents);
        builder.Property(permission => permission.Code).IsRequired().HasMaxLength(100);
        builder.Property(permission => permission.Name).IsRequired().HasMaxLength(150);
        builder.HasIndex(permission => permission.Code).IsUnique();
    }
}
