using Domain.Primitives;

namespace Domain.Events;

public sealed record CategoryUpdatedDomainEvent(Guid CategoryId, string Code, string Name, bool IsActive) : IDomainEvent;
