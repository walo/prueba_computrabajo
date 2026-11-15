namespace Application.Features.Products.DTOs;

public sealed record PagedProductsResponse(
    IReadOnlyList<ProductResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
