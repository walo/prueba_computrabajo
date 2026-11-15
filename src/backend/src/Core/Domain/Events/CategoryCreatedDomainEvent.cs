using Domain.Primitives;

namespace Domain.Events;

public sealed record CategoryCreatedDomainEvent(Guid CategoryId, string Code, string Name) : IDomainEvent;
