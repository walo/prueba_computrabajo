using Application.Common.Interfaces;
using Dapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository(
    AppDbContext dbContext,
    IDbConnectionFactory connectionFactory) : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default)
    {
        return dbContext.RefreshTokens.AsNoTracking().FirstOrDefaultAsync(token => token.TokenHash == tokenHash, ct);
    }

    public async Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        return await dbContext.RefreshTokens
            .AsNoTracking()
            .Where(token => token.UserId == userId && token.RevokedAtUtc == null && !token.IsUsed && token.ExpiresAtUtc > now)
            .ToListAsync(ct);
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        if (!dbContext.Database.IsRelational())
        {
            await dbContext.RefreshTokens.AddAsync(refreshToken, ct);

            return;
        }
        const string sql = """
            INSERT INTO RefreshTokens (Id, UserId, TokenHash, CreatedAtUtc, ExpiresAtUtc, RevokedAtUtc, ReplacedByTokenHash, IsUsed)
            VALUES (@Id, @UserId, @TokenHash, @CreatedAtUtc, @ExpiresAtUtc, @RevokedAtUtc, @ReplacedByTokenHash, @IsUsed)
            """;
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, refreshToken, cancellationToken: ct));
    }

    public async Task UpdateAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        if (!dbContext.Database.IsRelational())
        {
            dbContext.RefreshTokens.Update(refreshToken);

            return;
        }
        const string sql = """
            UPDATE RefreshTokens
            SET RevokedAtUtc = @RevokedAtUtc, ReplacedByTokenHash = @ReplacedByTokenHash, IsUsed = @IsUsed
            WHERE Id = @Id
            """;
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, refreshToken, cancellationToken: ct));
    }
}
