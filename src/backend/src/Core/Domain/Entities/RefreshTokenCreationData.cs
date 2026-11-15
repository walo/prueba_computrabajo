namespace Domain.Entities;

public sealed record RefreshTokenCreationData(
    Guid UserId,
    string TokenHash,
    int ExpirationDays);
