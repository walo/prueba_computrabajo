namespace Domain.ValueObjects;

public sealed record CategoryProductsQuery(
    Guid CategoryId,
    CategoryProductsFilter Filter);
