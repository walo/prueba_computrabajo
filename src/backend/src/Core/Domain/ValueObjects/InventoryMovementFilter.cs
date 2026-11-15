namespace Domain.ValueObjects;

public sealed record InventoryMovementFilter(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    Guid? ProductId = null,
    string? Type = null,
    DateTime? FromDateUtc = null,
    DateTime? ToDateUtc = null);
