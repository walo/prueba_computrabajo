using Application.Features.Auth.DTOs;
using ErrorOr;
using MediatR;

namespace Application.Features.Auth.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery(Guid UserId) : IRequest<ErrorOr<CurrentUserResponse>>;
