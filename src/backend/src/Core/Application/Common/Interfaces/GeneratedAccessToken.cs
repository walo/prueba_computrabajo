namespace Application.Common.Interfaces;

public sealed record GeneratedAccessToken(
    string Token,
    string Jti,
    DateTime ExpiresAtUtc);
