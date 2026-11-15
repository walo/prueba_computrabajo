namespace Domain.ValueObjects;

public sealed record ProductCreationData(
    string Sku,
    string Name,
    string? Description,
    Guid CategoryId,
    decimal Price,
    int InitialStock,
    int MinStock);
