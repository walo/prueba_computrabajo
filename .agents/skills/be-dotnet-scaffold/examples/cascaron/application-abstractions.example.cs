// NOTA: Generar 1 archivo físico (.cs) por interfaz en Core/Application/Common/Interfaces/:
// - IJwtProvider.cs
// - IPasswordHasher.cs
// - ICacheService.cs
// - IPermissionCacheService.cs
// - ITokenBlacklistService.cs
// - ICurrentUserContext.cs
// - IDateTimeProvider.cs
// - IUserRepository.cs
// - IRefreshTokenRepository.cs
// - IAuthFinder.cs

using Domain.Entities;
using Domain.Primitives;

namespace Application.Common.Interfaces;

public interface IJwtProvider
{
    int AccessTokenMinutes { get; }
    string GenerateAccessToken(Guid userId, string email, string role, out string jti, out DateTime expiresAt);
    (string Token, string Hash) GenerateRefreshToken();
    string HashToken(string token);
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = default, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
}

public interface IPermissionCacheService
{
    Task<HashSet<string>> GetPermissionsForUserAsync(Guid userId, CancellationToken ct = default);
    Task InvalidateUserPermissionsAsync(Guid userId, CancellationToken ct = default);
    Task InvalidateRolePermissionsAsync(Guid roleId, CancellationToken ct = default);
}

public interface ITokenBlacklistService
{
    Task BlacklistTokenAsync(string jti, DateTime expiresAtUtc, CancellationToken ct = default);
    Task<bool> IsTokenBlacklistedAsync(string jti, CancellationToken ct = default);
}

public interface ICurrentUserContext
{
    Guid? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}

public interface IUserRepository : IRepository<User, Guid>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
}

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default);
    Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default);
}

public interface IAuthFinder
{
    Task<(Guid UserId, string Email, string FullName, bool IsActive)?> GetUserIdentityByIdAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetPermissionNamesByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> GetUserIdsByRoleIdAsync(Guid roleId, CancellationToken ct = default);
}
