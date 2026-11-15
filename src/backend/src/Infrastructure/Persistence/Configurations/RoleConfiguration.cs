using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(role => role.Id);
        builder.Property(role => role.Id).ValueGeneratedNever();
        builder.Ignore(role => role.DomainEvents);
        builder.Property(role => role.Code).IsRequired().HasMaxLength(50);
        builder.Property(role => role.Name).IsRequired().HasMaxLength(100);
        builder.Property(role => role.Description).IsRequired().HasMaxLength(250);
        builder.HasIndex(role => role.Code).IsUnique();
    }
}
