using Application.Common.Interfaces;
using Domain.Primitives;
using ErrorOr;
using MediatR;

namespace Application.Features.Auth.Commands.Logout;

public sealed class LogoutCommandHandler(
    ITokenBlacklistService tokenBlacklistService,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<LogoutCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(LogoutCommand request, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(request.Jti))
        {
            await tokenBlacklistService.BlacklistTokenAsync(new BlacklistTokenRequest(request.Jti, request.ExpiresAtUtc), ct);
        }
        var activeTokens = await refreshTokenRepository.GetActiveByUserIdAsync(request.UserId, ct);
        foreach (var token in activeTokens)
        {
            token.Revoke();
            await refreshTokenRepository.UpdateAsync(token, ct);
        }
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success;
    }
}
