using Application.Common.Interfaces;
using Application.Features.Auth.DTOs;
using ErrorOr;
using MediatR;

namespace Application.Features.Auth.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler(
    IAuthFinder authFinder,
    IPermissionCacheService permissionCacheService)
    : IRequestHandler<GetCurrentUserQuery, ErrorOr<CurrentUserResponse>>
{
    public async Task<ErrorOr<CurrentUserResponse>> Handle(GetCurrentUserQuery request, CancellationToken ct)
    {
        var identity = await authFinder.GetUserIdentityByIdAsync(request.UserId, ct);
        if (identity is null)
        {
            return Error.NotFound("Auth.UserNotFound", "The session user does not exist.");
        }
        if (!identity.IsActive)
        {
            return Error.Forbidden("Auth.UserInactive", "The user account is inactive.");
        }
        var permissions = await permissionCacheService.GetPermissionsForUserAsync(request.UserId, ct);

        return new CurrentUserResponse(
            identity.UserId,
            identity.Email,
            identity.FullName,
            permissions.OrderBy(permission => permission).ToList());
    }
}
