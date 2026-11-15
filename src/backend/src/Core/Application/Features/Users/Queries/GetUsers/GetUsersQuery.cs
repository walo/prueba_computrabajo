using Application.Features.Users.DTOs;
using ErrorOr;
using MediatR;

namespace Application.Features.Users.Queries.GetUsers;

public sealed record GetUsersQuery : IRequest<ErrorOr<IReadOnlyList<UserSummaryResponse>>>;
