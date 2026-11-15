namespace Application.Features.InventoryMovements.DTOs;

public sealed record PagedInventoryMovementsResponse(
    IReadOnlyList<InventoryMovementResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
