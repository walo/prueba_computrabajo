using Domain.Primitives;

namespace Domain.Entities;

public sealed class RefreshToken : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }
    public bool IsUsed { get; private set; }
    public bool IsActive => RevokedAtUtc is null && !IsUsed && DateTime.UtcNow < ExpiresAtUtc;

    private RefreshToken()
    {
    }

    public static RefreshToken Create(RefreshTokenCreationData data)
    {
        var now = DateTime.UtcNow;

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = data.UserId,
            TokenHash = data.TokenHash,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddDays(data.ExpirationDays),
            IsUsed = false
        };
    }

    public void MarkAsUsed(string replacedByTokenHash)
    {
        IsUsed = true;
        RevokedAtUtc = DateTime.UtcNow;
        ReplacedByTokenHash = replacedByTokenHash;
    }

    public void Revoke()
    {
        RevokedAtUtc = DateTime.UtcNow;
    }
}
