// NOTA DE ARQUITECTURA: En el proyecto real, cada Command, Query, Validator, Handler y DTO DEBE residir en su propio archivo físico (.cs):
// - Features/Auth/Commands/Login/{LoginCommand.cs, LoginCommandValidator.cs, LoginCommandHandler.cs}
// - Features/Auth/Commands/RefreshToken/{RefreshTokenCommand.cs, RefreshTokenCommandValidator.cs, RefreshTokenCommandHandler.cs}
// - Features/Auth/Commands/Logout/{LogoutCommand.cs, LogoutCommandHandler.cs}
// - Features/Auth/Queries/GetCurrentUser/{GetCurrentUserQuery.cs, GetCurrentUserQueryHandler.cs}

using Application.Common.Interfaces;
using Application.Features.Auth.DTOs;
using Domain.Primitives;
using ErrorOr;
using MediatR;
using RefreshTokenEntity = Domain.Entities.RefreshToken;

namespace Application.Features.Auth.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery(Guid UserId) : IRequest<ErrorOr<CurrentUserResponse>>;

public sealed class GetCurrentUserQueryHandler(
    IAuthFinder authFinder,
    IPermissionCacheService permissionCacheService)
    : IRequestHandler<GetCurrentUserQuery, ErrorOr<CurrentUserResponse>>
{
    public async Task<ErrorOr<CurrentUserResponse>> Handle(GetCurrentUserQuery request, CancellationToken ct)
    {
        var identity = await authFinder.GetUserIdentityByIdAsync(request.UserId, ct);
        if (identity is null)
            return Error.NotFound("Auth.UserNotFound", "El usuario de la sesión no existe.");

        if (!identity.Value.IsActive)
            return Error.Forbidden("Auth.UserInactive", "La cuenta de usuario está desactivada.");

        var permissions = await permissionCacheService.GetPermissionsForUserAsync(request.UserId, ct);

        return new CurrentUserResponse(
            identity.Value.UserId,
            identity.Value.Email,
            identity.Value.FullName,
            permissions.OrderBy(p => p).ToList());
    }
}
