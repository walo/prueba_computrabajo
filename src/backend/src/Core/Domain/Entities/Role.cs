using Domain.Primitives;

namespace Domain.Entities;

public sealed class Role : Entity<Guid>
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    private Role()
    {
    }

    public static Role Create(RoleCreationData data)
    {
        return new Role
        {
            Id = Guid.NewGuid(),
            Code = data.Code.Trim().ToUpperInvariant(),
            Name = data.Name.Trim(),
            Description = data.Description.Trim()
        };
    }
}
