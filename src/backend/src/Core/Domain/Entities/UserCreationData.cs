namespace Domain.Entities;

public sealed record UserCreationData(
    string FullName,
    string Email,
    string PasswordHash,
    Guid RoleId);
