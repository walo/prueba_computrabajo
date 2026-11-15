namespace Application.Features.Products.DTOs;

public sealed record ProductResponse(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    Guid CategoryId,
    string CategoryCode,
    string CategoryName,
    decimal Price,
    int Stock,
    int MinStock,
    bool IsLowStock,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
