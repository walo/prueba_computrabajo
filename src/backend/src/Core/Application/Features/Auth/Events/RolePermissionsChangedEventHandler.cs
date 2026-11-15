using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Auth.Events;

public sealed class RolePermissionsChangedEventHandler(IPermissionCacheService permissionCacheService)
    : INotificationHandler<RolePermissionsChangedNotification>
{
    public Task Handle(RolePermissionsChangedNotification notification, CancellationToken ct)
    {
        return permissionCacheService.InvalidateRolePermissionsAsync(notification.RoleId, ct);
    }
}
