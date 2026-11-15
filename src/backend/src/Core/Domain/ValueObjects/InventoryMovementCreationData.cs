namespace Domain.ValueObjects;

public sealed record InventoryMovementCreationData(
    Guid ProductId,
    string Type,
    int Quantity,
    int PreviousStock,
    string Reason,
    Guid PerformedByUserId);
