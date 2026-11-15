using Domain.Primitives;

namespace Domain.Entities;

public sealed class User : AggregateRoot<Guid>
{
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public Guid RoleId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    private User()
    {
    }

    public static User Create(UserCreationData data)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FullName = data.FullName.Trim(),
            Email = data.Email.Trim().ToLowerInvariant(),
            PasswordHash = data.PasswordHash,
            RoleId = data.RoleId,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
