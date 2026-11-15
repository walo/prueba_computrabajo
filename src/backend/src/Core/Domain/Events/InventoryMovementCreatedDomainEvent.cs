using Domain.Primitives;

namespace Domain.Events;

public sealed record InventoryMovementCreatedDomainEvent(
    Guid MovementId,
    Guid ProductId,
    string Type,
    int Quantity,
    int PreviousStock,
    int NewStock,
    Guid PerformedByUserId) : IDomainEvent;
