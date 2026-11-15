namespace Application.Features.InventoryMovements.DTOs;

public sealed record InventoryMovementResponse(
    Guid Id,
    Guid ProductId,
    string ProductSku,
    string ProductName,
    string Type,
    int Quantity,
    int PreviousStock,
    int NewStock,
    string Reason,
    Guid PerformedByUserId,
    string PerformedByEmail,
    DateTime CreatedAtUtc);
