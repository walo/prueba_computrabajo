using Domain.Primitives;

namespace Domain.Events;

public sealed record CategoryDeletedDomainEvent(Guid CategoryId, string Code) : IDomainEvent;
