using Application.Common.Interfaces;
using Application.Features.Auth.DTOs;
using Domain.Entities;
using Domain.Primitives;
using ErrorOr;
using MediatR;
using RefreshTokenEntity = Domain.Entities.RefreshToken;

namespace Application.Features.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IAuthFinder authFinder,
    IJwtProvider jwtProvider,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RefreshTokenCommand, ErrorOr<AuthTokensResponse>>
{
    public async Task<ErrorOr<AuthTokensResponse>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var tokenHash = jwtProvider.HashToken(request.RefreshToken);
        var existingToken = await refreshTokenRepository.GetByHashAsync(tokenHash, ct);
        if (existingToken is null)
        {
            return Error.Unauthorized("Auth.InvalidRefreshToken", "The refresh token is invalid.");
        }
        if (existingToken.IsUsed)
        {
            await RevokeAllActiveTokensAsync(existingToken.UserId, ct);

            return Error.Unauthorized("Auth.RefreshTokenReused", "Refresh token reuse detected. All sessions revoked.");
        }
        if (!existingToken.IsActive)
        {
            return Error.Unauthorized("Auth.RefreshTokenExpired", "The refresh token has expired or been revoked.");
        }

        return await RotateRefreshTokenAsync(existingToken, ct);
    }

    private async Task<ErrorOr<AuthTokensResponse>> RotateRefreshTokenAsync(RefreshTokenEntity existingToken, CancellationToken ct)
    {
        var identity = await authFinder.GetUserIdentityByIdAsync(existingToken.UserId, ct);
        if (identity is null || !identity.IsActive)
        {
            return Error.Unauthorized("Auth.UserUnavailable", "The user account is unavailable or inactive.");
        }
        var newAccessToken = jwtProvider.GenerateAccessToken(new JwtTokenRequest(identity.UserId, identity.Email, identity.RoleCode));
        var newRefreshToken = jwtProvider.GenerateRefreshToken();
        existingToken.MarkAsUsed(newRefreshToken.Hash);
        await refreshTokenRepository.UpdateAsync(existingToken, ct);
        var nextEntity = RefreshTokenEntity.Create(new RefreshTokenCreationData(identity.UserId, newRefreshToken.Hash, 7));
        await refreshTokenRepository.AddAsync(nextEntity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new AuthTokensResponse(newAccessToken.Token, newRefreshToken.Token, newAccessToken.ExpiresAtUtc);
    }

    private async Task RevokeAllActiveTokensAsync(Guid userId, CancellationToken ct)
    {
        var activeTokens = await refreshTokenRepository.GetActiveByUserIdAsync(userId, ct);
        foreach (var token in activeTokens)
        {
            token.Revoke();
            await refreshTokenRepository.UpdateAsync(token, ct);
        }
        await unitOfWork.SaveChangesAsync(ct);
    }
}
