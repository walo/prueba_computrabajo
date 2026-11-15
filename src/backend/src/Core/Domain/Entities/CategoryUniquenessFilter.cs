namespace Domain.Entities;

public sealed record CategoryUniquenessFilter(string Code, string Name, Guid? ExcludedId = null);
