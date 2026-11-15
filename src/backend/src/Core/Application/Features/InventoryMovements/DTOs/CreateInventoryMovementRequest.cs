namespace Application.Features.InventoryMovements.DTOs;

public sealed record CreateInventoryMovementRequest(
    Guid ProductId,
    string Type,
    int Quantity,
    string Reason);
