using Application.Features.Auth.DTOs;
using ErrorOr;
using MediatR;

namespace Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(
    string Email,
    string Password) : IRequest<ErrorOr<AuthTokensResponse>>;
