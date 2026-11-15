// NOTA: Generar 1 archivo físico por clase dentro de src/Infrastructure/Authentication/:
// - JwtProvider.cs
// - PasswordHasher.cs
// - TokenBlacklistService.cs
// - PermissionCacheService.cs (consume IAuthFinder e ICacheService; CERO referencias a Infrastructure.Persistence)
// - DependencyInjection.cs

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Authentication;

public sealed class JwtProvider(IConfiguration configuration) : IJwtProvider
{
    private readonly string _secretKey = configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey missing.");
    private readonly string _issuer = configuration["Jwt:Issuer"] ?? "EnterpriseApp";
    private readonly string _audience = configuration["Jwt:Audience"] ?? "EnterpriseClient";

    public int AccessTokenMinutes { get; } =
        int.TryParse(configuration["Jwt:AccessTokenMinutes"], out var m) ? m : 15;

    public string GenerateAccessToken(Guid userId, string email, string role, out string jti, out DateTime expiresAt)
    {
        jti = Guid.NewGuid().ToString("N");
        expiresAt = DateTime.UtcNow.AddMinutes(AccessTokenMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim("token_type", "access")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (string Token, string Hash) GenerateRefreshToken()
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        return (token, HashToken(token));
    }

    public string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();
}

public sealed class PermissionCacheService(ICacheService cacheService, IAuthFinder authFinder) : IPermissionCacheService
{
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(60);

    public async Task<HashSet<string>> GetPermissionsForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var cacheKey = $"permissions:user:{userId}";
        var cached = await cacheService.GetAsync<List<string>>(cacheKey, ct);
        if (cached is not null)
            return cached.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var permissions = await authFinder.GetPermissionNamesByUserIdAsync(userId, ct);
        await cacheService.SetAsync(cacheKey, permissions.ToList(), DefaultTtl, ct);
        return permissions.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public Task InvalidateUserPermissionsAsync(Guid userId, CancellationToken ct = default) =>
        cacheService.RemoveAsync($"permissions:user:{userId}", ct);

    public async Task InvalidateRolePermissionsAsync(Guid roleId, CancellationToken ct = default)
    {
        var userIds = await authFinder.GetUserIdsByRoleIdAsync(roleId, ct);
        foreach (var userId in userIds)
            await InvalidateUserPermissionsAsync(userId, ct);
    }
}
