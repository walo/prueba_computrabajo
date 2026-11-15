namespace Domain.Entities;

public sealed class RolePermission
{
    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }

    private RolePermission()
    {
    }

    public static RolePermission Create(RolePermissionCreationData data)
    {
        return new RolePermission
        {
            RoleId = data.RoleId,
            PermissionId = data.PermissionId
        };
    }
}
