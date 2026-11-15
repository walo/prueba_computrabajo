using Domain.Primitives;

namespace Domain.Entities;

public sealed class Permission : Entity<Guid>
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;

    private Permission()
    {
    }

    public static Permission Create(PermissionCreationData data)
    {
        return new Permission
        {
            Id = Guid.NewGuid(),
            Code = data.Code.Trim(),
            Name = data.Name.Trim()
        };
    }
}
