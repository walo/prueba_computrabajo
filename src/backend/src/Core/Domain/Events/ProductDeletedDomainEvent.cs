using Domain.Primitives;

namespace Domain.Events;

public sealed record ProductDeletedDomainEvent(
    Guid ProductId,
    string Sku) : IDomainEvent;
