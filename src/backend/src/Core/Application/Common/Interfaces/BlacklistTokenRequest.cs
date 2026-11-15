namespace Application.Common.Interfaces;

public sealed record BlacklistTokenRequest(
    string Jti,
    DateTime ExpiresAtUtc);
