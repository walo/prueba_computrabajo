using Application.Common.Interfaces;
using Application.Features.Auth.DTOs;
using Domain.Entities;
using Domain.Primitives;
using ErrorOr;
using MediatR;
using RefreshTokenEntity = Domain.Entities.RefreshToken;

namespace Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IAuthFinder authFinder,
    IJwtProvider jwtProvider,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
    : IRequestHandler<LoginCommand, ErrorOr<AuthTokensResponse>>
{
    public async Task<ErrorOr<AuthTokensResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, ct);
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password.");
        }
        if (!user.IsActive)
        {
            return Error.Forbidden("Auth.UserInactive", "The user account is inactive.");
        }
        var identity = await authFinder.GetUserIdentityByIdAsync(user.Id, ct);
        var roleCode = identity?.RoleCode ?? "USER";
        var accessToken = jwtProvider.GenerateAccessToken(new JwtTokenRequest(user.Id, user.Email, roleCode));
        var refreshToken = jwtProvider.GenerateRefreshToken();
        var tokenEntity = RefreshTokenEntity.Create(new RefreshTokenCreationData(user.Id, refreshToken.Hash, 7));
        await refreshTokenRepository.AddAsync(tokenEntity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new AuthTokensResponse(accessToken.Token, refreshToken.Token, accessToken.ExpiresAtUtc);
    }
}
