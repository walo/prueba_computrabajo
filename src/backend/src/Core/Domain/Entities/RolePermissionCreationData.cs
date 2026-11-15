namespace Domain.Entities;

public sealed record RolePermissionCreationData(
    Guid RoleId,
    Guid PermissionId);
