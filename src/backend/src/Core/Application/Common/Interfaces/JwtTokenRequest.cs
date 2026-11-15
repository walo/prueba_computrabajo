namespace Application.Common.Interfaces;

public sealed record JwtTokenRequest(
    Guid UserId,
    string Email,
    string RoleCode);
