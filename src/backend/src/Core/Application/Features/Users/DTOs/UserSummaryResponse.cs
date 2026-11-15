namespace Application.Features.Users.DTOs;

public sealed record UserSummaryResponse(
    Guid UserId,
    string FullName,
    string Email,
    string RoleName,
    bool IsActive,
    DateTime CreatedAtUtc);
