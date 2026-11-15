namespace Application.Features.Products.DTOs;

public sealed record UpdateProductRequest(
    string Sku,
    string Name,
    string? Description,
    Guid CategoryId,
    decimal Price,
    int MinStock,
    bool IsActive);
