namespace Domain.ValueObjects;

public sealed record ProductUpdateData(
    string Sku,
    string Name,
    string? Description,
    Guid CategoryId,
    decimal Price,
    int MinStock,
    bool IsActive);
