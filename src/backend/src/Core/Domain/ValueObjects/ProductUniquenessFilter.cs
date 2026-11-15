namespace Domain.ValueObjects;

public sealed record ProductUniquenessFilter(
    string Sku,
    Guid CategoryId,
    Guid? ExcludedId = null);
