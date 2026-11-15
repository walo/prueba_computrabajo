using Application.Features.Auth.DTOs;
using ErrorOr;
using MediatR;

namespace Application.Features.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<ErrorOr<AuthTokensResponse>>;
