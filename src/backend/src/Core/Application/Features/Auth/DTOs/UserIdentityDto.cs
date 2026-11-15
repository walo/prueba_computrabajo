namespace Application.Features.Auth.DTOs;

public sealed record UserIdentityDto(
    Guid UserId,
    string Email,
    string FullName,
    string RoleCode,
    bool IsActive);
