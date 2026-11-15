using Application.Common.Interfaces;
using Dapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class UserRepository(
    AppDbContext dbContext,
    IDbConnectionFactory connectionFactory) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return dbContext.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Id == id, ct);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return dbContext.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Email == normalizedEmail, ct);
    }

    public async Task AddAsync(User entity, CancellationToken ct = default)
    {
        if (!dbContext.Database.IsRelational())
        {
            await dbContext.Users.AddAsync(entity, ct);

            return;
        }
        const string sql = """
            INSERT INTO Users (Id, FullName, Email, PasswordHash, RoleId, IsActive, CreatedAtUtc)
            VALUES (@Id, @FullName, @Email, @PasswordHash, @RoleId, @IsActive, @CreatedAtUtc)
            """;
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, entity, cancellationToken: ct));
    }

    public async Task UpdateAsync(User entity, CancellationToken ct = default)
    {
        if (!dbContext.Database.IsRelational())
        {
            dbContext.Users.Update(entity);

            return;
        }
        const string sql = """
            UPDATE Users
            SET FullName = @FullName, Email = @Email, PasswordHash = @PasswordHash, RoleId = @RoleId, IsActive = @IsActive
            WHERE Id = @Id
            """;
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, entity, cancellationToken: ct));
    }

    public async Task DeleteAsync(User entity, CancellationToken ct = default)
    {
        if (!dbContext.Database.IsRelational())
        {
            dbContext.Users.Remove(entity);

            return;
        }
        const string sql = "DELETE FROM Users WHERE Id = @Id";
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { entity.Id }, cancellationToken: ct));
    }
}
