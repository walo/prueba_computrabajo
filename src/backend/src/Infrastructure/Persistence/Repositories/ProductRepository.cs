using Application.Common.Interfaces;
using Dapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class ProductRepository(
    AppDbContext dbContext,
    IDbConnectionFactory connectionFactory) : IProductRepository
{
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return dbContext.Products.AsNoTracking().FirstOrDefaultAsync(product => product.Id == id, ct);
    }

    public Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default)
    {
        var normalizedSku = sku.Trim().ToUpperInvariant();

        return dbContext.Products.AsNoTracking().FirstOrDefaultAsync(product => product.Sku == normalizedSku, ct);
    }

    public async Task<bool> HasInventoryMovementsAsync(Guid productId, CancellationToken ct = default)
    {
        if (!dbContext.Database.IsRelational())
        {
            return await dbContext.InventoryMovements.AsNoTracking().AnyAsync(movement => movement.ProductId == productId, ct);
        }
        const string tableCheckSql = "SELECT CASE WHEN OBJECT_ID(N'dbo.InventoryMovements', N'U') IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END";
        using var connection = connectionFactory.CreateConnection();
        var movementsTableExists = await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(tableCheckSql, cancellationToken: ct));
        if (!movementsTableExists)
        {
            return false;
        }
        const string countSql = "SELECT CASE WHEN EXISTS (SELECT 1 FROM InventoryMovements WHERE ProductId = @ProductId) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END";

        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(countSql, new { ProductId = productId }, cancellationToken: ct));
    }

    public async Task AddAsync(Product product, CancellationToken ct = default)
    {
        if (!dbContext.Database.IsRelational())
        {
            await dbContext.Products.AddAsync(product, ct);

            return;
        }
        const string sql = """
            INSERT INTO Products (Id, Sku, Name, Description, CategoryId, Price, Stock, MinStock, IsActive, CreatedAtUtc, UpdatedAtUtc)
            VALUES (@Id, @Sku, @Name, @Description, @CategoryId, @Price, @Stock, @MinStock, @IsActive, @CreatedAtUtc, @UpdatedAtUtc)
            """;
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, product, cancellationToken: ct));
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        if (!dbContext.Database.IsRelational())
        {
            dbContext.Products.Update(product);

            return;
        }
        const string sql = """
            UPDATE Products
            SET Sku = @Sku, Name = @Name, Description = @Description, CategoryId = @CategoryId,
                Price = @Price, Stock = @Stock, MinStock = @MinStock, IsActive = @IsActive, UpdatedAtUtc = @UpdatedAtUtc
            WHERE Id = @Id
            """;
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, product, cancellationToken: ct));
    }

    public async Task DeleteAsync(Product product, CancellationToken ct = default)
    {
        if (!dbContext.Database.IsRelational())
        {
            dbContext.Products.Remove(product);

            return;
        }
        const string sql = "DELETE FROM Products WHERE Id = @Id";
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { product.Id }, cancellationToken: ct));
    }
}
