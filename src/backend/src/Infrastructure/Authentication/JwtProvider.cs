using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Common.Interfaces;
using Domain.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Authentication;

public sealed class JwtProvider(IConfiguration configuration) : IJwtProvider
{
    private readonly string _secretKey = configuration["JWT_SECRET_KEY"]
        ?? configuration["Jwt:SecretKey"]
        ?? throw new InvalidOperationException("JWT secret key is not configured.");
    private readonly string _issuer = configuration["JWT_ISSUER"]
        ?? configuration["Jwt:Issuer"]
        ?? "RedarborInventoryApi";
    private readonly string _audience = configuration["JWT_AUDIENCE"]
        ?? configuration["Jwt:Audience"]
        ?? "RedarborInventoryClient";

    public int AccessTokenMinutes { get; } = int.TryParse(
        configuration["JWT_ACCESS_TOKEN_MINUTES"] ?? configuration["Jwt:AccessTokenMinutes"],
        out var minutes) ? minutes : 15;

    public GeneratedAccessToken GenerateAccessToken(JwtTokenRequest request)
    {
        var jti = Guid.NewGuid().ToString("N");
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(AccessTokenMinutes);
        var claims = BuildClaims(request, jti);
        var credentials = BuildSigningCredentials();
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);
        var encodedToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new GeneratedAccessToken(encodedToken, jti, expiresAtUtc);
    }

    public GeneratedRefreshToken GenerateRefreshToken()
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        return new GeneratedRefreshToken(token, HashToken(token));
    }

    public string HashToken(string token)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();
    }

    private static Claim[] BuildClaims(JwtTokenRequest request, string jti)
    {
        return
        [
            new Claim(JwtRegisteredClaimNames.Sub, request.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, request.Email),
            new Claim(ClaimTypes.Role, request.RoleCode),
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim("token_type", TokenTypes.AccessToken)
        ];
    }

    private SigningCredentials BuildSigningCredentials()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));

        return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }
}
