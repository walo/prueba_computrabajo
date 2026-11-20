namespace Domain.ValueObjects;

public sealed record CategoryProductsFilter(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    bool? IsActive = null,
    bool? LowStockOnly = null);
