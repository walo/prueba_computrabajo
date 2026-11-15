namespace Domain.ValueObjects;

public sealed record ProductFilter(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    Guid? CategoryId = null,
    bool? IsActive = null,
    bool? LowStockOnly = null);
