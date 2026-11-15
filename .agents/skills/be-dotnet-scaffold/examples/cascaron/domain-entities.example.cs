using Domain.Primitives;

namespace Domain.Enums;

public static class TokenType
{
    public const string AccessToken = "access_token";
    public const string RefreshToken = "refresh_token";
}

public static class PermissionType
{
    // Usuarios
    public const string UsersRead = "Users.Read";
    public const string UsersCreate = "Users.Create";
    public const string UsersUpdate = "Users.Update";
    public const string UsersDelete = "Users.Delete";

    // Roles
    public const string RolesRead = "Roles.Read";
    public const string RolesCreate = "Roles.Create";
    public const string RolesUpdate = "Roles.Update";
    public const string RolesDelete = "Roles.Delete";

    // Módulos de Features / Negocio (Extensible)
    public const string FeaturesRead = "Features.Read";
    public const string FeaturesCreate = "Features.Create";
    public const string FeaturesUpdate = "Features.Update";
    public const string FeaturesDelete = "Features.Delete";
}

namespace Domain.Events;

public sealed record RolePermissionsChangedDomainEvent(Guid RoleId) : IDomainEvent;

namespace Domain.Entities.Auth;

public sealed class RefreshToken : Entity
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }
    public bool IsUsed { get; private set; }

    public bool IsActive => RevokedAt == null && !IsUsed && DateTime.UtcNow < ExpiresAt;

    private RefreshToken() { } // EF Core

    public static RefreshToken Create(Guid userId, string tokenHash, int expirationDays)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
            IsUsed = false
        };
    }

    public void MarkAsUsed(string replacedByHash)
    {
        IsUsed = true;
        RevokedAt = DateTime.UtcNow;
        ReplacedByTokenHash = replacedByHash;
    }

    public void Revoke()
    {
        RevokedAt = DateTime.UtcNow;
    }
}

namespace Domain.Entities.Users;

public sealed class User : AggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public Guid CompanyId { get; private set; }
    public Guid RoleId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public Role? Role { get; private set; }

    private User() { } // EF Core

    public static User Create(string name, string email, string passwordHash, Guid roleId, Guid companyId)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            PasswordHash = passwordHash,
            RoleId = roleId,
            CompanyId = companyId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}

namespace Domain.Entities.Roles;

public sealed class Role : Entity<Guid>
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public ICollection<RolePermission> RolePermissions { get; private set; } = [];

    private Role() { }

    public static Role Create(string code, string name, string description)
    {
        return new Role
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            Description = description
        };
    }
}

namespace Domain.Entities.Permissions;

public sealed class Permission : Entity<Guid>
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;

    private Permission() { }

    public static Permission Create(string code, string name)
    {
        return new Permission
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name
        };
    }
}

public sealed class RolePermission
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }

    public Role? Role { get; set; }
    public Permission? Permission { get; set; }
}
