using Domain.Primitives;

namespace Domain.Events;

public sealed record ProductCreatedDomainEvent(
    Guid ProductId,
    string Sku,
    string Name,
    Guid CategoryId) : IDomainEvent;
