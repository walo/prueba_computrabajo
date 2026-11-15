using Domain.Primitives;

namespace Domain.Events;

public sealed record RolePermissionsChangedDomainEvent(Guid RoleId) : IDomainEvent;
