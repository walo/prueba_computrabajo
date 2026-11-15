using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(category => category.Id);
        builder.Property(category => category.Id).ValueGeneratedNever();
        builder.Ignore(category => category.DomainEvents);
        builder.Property(category => category.Code).IsRequired().HasMaxLength(30);
        builder.Property(category => category.Name).IsRequired().HasMaxLength(100);
        builder.Property(category => category.Description).IsRequired(false).HasMaxLength(500);
        builder.Property(category => category.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(category => category.CreatedAtUtc).IsRequired();
        builder.Property(category => category.UpdatedAtUtc).IsRequired(false);
        builder.HasIndex(category => category.Code).IsUnique();
        builder.HasIndex(category => category.Name).IsUnique();
    }
}
