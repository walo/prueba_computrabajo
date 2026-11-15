using Domain.Primitives;

namespace Domain.Events;

public sealed record ProductUpdatedDomainEvent(
    Guid ProductId,
    string Sku,
    string Name,
    Guid CategoryId,
    bool IsActive) : IDomainEvent;
