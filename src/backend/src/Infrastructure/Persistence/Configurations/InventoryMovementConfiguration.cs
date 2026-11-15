using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
{
    public void Configure(EntityTypeBuilder<InventoryMovement> builder)
    {
        builder.ToTable("InventoryMovements");
        builder.HasKey(movement => movement.Id);
        builder.Property(movement => movement.Id).ValueGeneratedNever();
        builder.Ignore(movement => movement.DomainEvents);
        ConfigureColumns(builder);
        ConfigureIndexes(builder);
    }

    private static void ConfigureColumns(EntityTypeBuilder<InventoryMovement> builder)
    {
        builder.Property(movement => movement.ProductId).IsRequired();
        builder.Property(movement => movement.Type).IsRequired().HasMaxLength(10);
        builder.Property(movement => movement.Quantity).IsRequired();
        builder.Property(movement => movement.PreviousStock).IsRequired();
        builder.Property(movement => movement.NewStock).IsRequired();
        builder.Property(movement => movement.Reason).IsRequired().HasMaxLength(300);
        builder.Property(movement => movement.PerformedByUserId).IsRequired();
        builder.Property(movement => movement.CreatedAtUtc).IsRequired();
    }

    private static void ConfigureIndexes(EntityTypeBuilder<InventoryMovement> builder)
    {
        builder.HasIndex(movement => movement.ProductId).HasDatabaseName("IX_InventoryMovements_ProductId");
        builder.HasIndex(movement => movement.CreatedAtUtc).HasDatabaseName("IX_InventoryMovements_CreatedAtUtc");
    }
}
