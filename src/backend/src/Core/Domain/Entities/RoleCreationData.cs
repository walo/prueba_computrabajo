namespace Domain.Entities;

public sealed record RoleCreationData(
    string Code,
    string Name,
    string Description);
