namespace Application.Features.Auth.DTOs;

public sealed record CurrentUserResponse(
    Guid UserId,
    string Email,
    string FullName,
    IReadOnlyList<string> Permissions);
