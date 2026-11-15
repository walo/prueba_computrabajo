using Application.Common.Interfaces;
using Dapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository(
    AppDbContext dbContext,
    IDbConnectionFactory connectionFactory) : ICategoryRepository
{
    public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return dbContext.Categories.AsNoTracking().FirstOrDefaultAsync(category => category.Id == id, ct);
    }

    public Task<Category?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        var normalizedCode = code.Trim().ToLowerInvariant();

        return dbContext.Categories.AsNoTracking().FirstOrDefaultAsync(category => category.Code == normalizedCode, ct);
    }

    public Task<Category?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        var normalizedName = name.Trim().ToLowerInvariant();

        return dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(category => category.Name.ToLower() == normalizedName, ct);
    }

    public async Task<bool> HasAssociatedProductsAsync(Guid categoryId, CancellationToken ct = default)
    {
        if (!dbContext.Database.IsRelational())
        {
            return await dbContext.Products
                .AsNoTracking()
                .AnyAsync(product => product.CategoryId == categoryId, ct);
        }
        const string tableCheckSql = "SELECT CASE WHEN OBJECT_ID(N'dbo.Products', N'U') IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END";
        using var connection = connectionFactory.CreateConnection();
        var productsTableExists = await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(tableCheckSql, cancellationToken: ct));
        if (!productsTableExists)
        {
            return false;
        }
        const string countSql = "SELECT CASE WHEN EXISTS (SELECT 1 FROM Products WHERE CategoryId = @CategoryId) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END";

        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(countSql, new { CategoryId = categoryId }, cancellationToken: ct));
    }

    public async Task AddAsync(Category entity, CancellationToken ct = default)
    {
        if (!dbContext.Database.IsRelational())
        {
            await dbContext.Categories.AddAsync(entity, ct);

            return;
        }
        const string sql = """
            INSERT INTO Categories (Id, Code, Name, Description, IsActive, CreatedAtUtc, UpdatedAtUtc)
            VALUES (@Id, @Code, @Name, @Description, @IsActive, @CreatedAtUtc, @UpdatedAtUtc)
            """;
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, entity, cancellationToken: ct));
    }

    public async Task UpdateAsync(Category entity, CancellationToken ct = default)
    {
        if (!dbContext.Database.IsRelational())
        {
            dbContext.Categories.Update(entity);

            return;
        }
        const string sql = """
            UPDATE Categories
            SET Code = @Code, Name = @Name, Description = @Description, IsActive = @IsActive, UpdatedAtUtc = @UpdatedAtUtc
            WHERE Id = @Id
            """;
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, entity, cancellationToken: ct));
    }

    public async Task DeleteAsync(Category entity, CancellationToken ct = default)
    {
        if (!dbContext.Database.IsRelational())
        {
            dbContext.Categories.Remove(entity);

            return;
        }
        const string sql = "DELETE FROM Categories WHERE Id = @Id";
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { entity.Id }, cancellationToken: ct));
    }
}
