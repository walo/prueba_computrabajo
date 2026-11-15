namespace Application.Features.Products.DTOs;

public sealed record CreateProductRequest(
    string Sku,
    string Name,
    string? Description,
    Guid CategoryId,
    decimal Price,
    int InitialStock,
    int MinStock);
