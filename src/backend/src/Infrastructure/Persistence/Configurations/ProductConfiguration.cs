using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(product => product.Id);
        builder.Property(product => product.Id).ValueGeneratedNever();
        builder.Ignore(product => product.DomainEvents);
        ConfigureColumns(builder);
        ConfigureIndexes(builder);
    }

    private static void ConfigureColumns(EntityTypeBuilder<Product> builder)
    {
        builder.Property(product => product.Sku).IsRequired().HasMaxLength(30);
        builder.Property(product => product.Name).IsRequired().HasMaxLength(150);
        builder.Property(product => product.Description).IsRequired(false).HasMaxLength(500);
        builder.Property(product => product.CategoryId).IsRequired();
        builder.Property(product => product.Price).IsRequired().HasPrecision(18, 2);
        builder.Property(product => product.Stock).IsRequired();
        builder.Property(product => product.MinStock).IsRequired();
        builder.Property(product => product.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(product => product.CreatedAtUtc).IsRequired();
        builder.Property(product => product.UpdatedAtUtc).IsRequired(false);
    }

    private static void ConfigureIndexes(EntityTypeBuilder<Product> builder)
    {
        builder.HasIndex(product => product.Sku).IsUnique().HasDatabaseName("IX_Products_Sku");
        builder.HasIndex(product => product.CategoryId).HasDatabaseName("IX_Products_CategoryId");
    }
}
