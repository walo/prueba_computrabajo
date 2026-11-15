using System.Data;
using Application.Common.Interfaces;
using Dapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class InventoryMovementRepository(
    AppDbContext dbContext,
    IDbConnectionFactory connectionFactory) : IInventoryMovementRepository
{
    public async Task<bool> AddAsync(InventoryMovement movement, CancellationToken ct = default)
    {
        if (!dbContext.Database.IsRelational())
        {
            return await PersistInMemoryAsync(movement, ct);
        }

        return await PersistRelationalAsync(movement, ct);
    }

    private async Task<bool> PersistInMemoryAsync(InventoryMovement movement, CancellationToken ct)
    {
        var product = await dbContext.Products.FirstOrDefaultAsync(item => item.Id == movement.ProductId, ct);
        if (product is null || !product.IsActive || product.Stock != movement.PreviousStock)
        {
            return false;
        }
        product.AdjustStock(movement.NewStock);
        await dbContext.InventoryMovements.AddAsync(movement, ct);

        return true;
    }

    private async Task<bool> PersistRelationalAsync(InventoryMovement movement, CancellationToken ct)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var updatedRows = await TryUpdateProductStockAsync(transaction, movement, ct);
        if (updatedRows == 0)
        {
            transaction.Rollback();

            return false;
        }
        await InsertMovementAsync(transaction, movement, ct);
        transaction.Commit();

        return true;
    }

    private static Task<int> TryUpdateProductStockAsync(
        IDbTransaction transaction,
        InventoryMovement movement,
        CancellationToken ct)
    {
        const string updateSql = """
            UPDATE Products
            SET Stock = @NewStock, UpdatedAtUtc = @CreatedAtUtc
            WHERE Id = @ProductId AND Stock = @PreviousStock AND IsActive = 1
            """;

        return transaction.Connection!.ExecuteAsync(
            new CommandDefinition(updateSql, movement, transaction, cancellationToken: ct));
    }

    private static Task InsertMovementAsync(
        IDbTransaction transaction,
        InventoryMovement movement,
        CancellationToken ct)
    {
        const string insertSql = """
            INSERT INTO InventoryMovements (Id, ProductId, Type, Quantity, PreviousStock, NewStock, Reason, PerformedByUserId, CreatedAtUtc)
            VALUES (@Id, @ProductId, @Type, @Quantity, @PreviousStock, @NewStock, @Reason, @PerformedByUserId, @CreatedAtUtc)
            """;

        return transaction.Connection!.ExecuteAsync(
            new CommandDefinition(insertSql, movement, transaction, cancellationToken: ct));
    }
}
