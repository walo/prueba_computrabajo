namespace Application.Common.Interfaces;

public interface IJwtProvider
{
    int AccessTokenMinutes { get; }
    GeneratedAccessToken GenerateAccessToken(JwtTokenRequest request);
    GeneratedRefreshToken GenerateRefreshToken();
    string HashToken(string token);
}
