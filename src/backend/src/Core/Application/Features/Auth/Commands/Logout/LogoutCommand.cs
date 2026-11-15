using ErrorOr;
using MediatR;

namespace Application.Features.Auth.Commands.Logout;

public sealed record LogoutCommand(
    Guid UserId,
    string Jti,
    DateTime ExpiresAtUtc) : IRequest<ErrorOr<Success>>;
