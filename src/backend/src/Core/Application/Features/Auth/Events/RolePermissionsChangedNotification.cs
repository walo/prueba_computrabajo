using MediatR;

namespace Application.Features.Auth.Events;

public sealed record RolePermissionsChangedNotification(Guid RoleId) : INotification;
