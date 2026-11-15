namespace Domain.Entities;

public sealed record CategoryUpdateData(string Code, string Name, string? Description, bool IsActive);
