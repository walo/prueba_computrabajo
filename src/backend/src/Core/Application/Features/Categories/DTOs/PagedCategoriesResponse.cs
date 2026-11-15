namespace Application.Features.Categories.DTOs;

public sealed record PagedCategoriesResponse(
    IReadOnlyList<CategoryResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
