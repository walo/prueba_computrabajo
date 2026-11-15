using Application.Common.Interfaces;
using Application.Features.Users.DTOs;
using ErrorOr;
using MediatR;

namespace Application.Features.Users.Queries.GetUsers;

public sealed class GetUsersQueryHandler(IUserFinder userFinder)
    : IRequestHandler<GetUsersQuery, ErrorOr<IReadOnlyList<UserSummaryResponse>>>
{
    public async Task<ErrorOr<IReadOnlyList<UserSummaryResponse>>> Handle(GetUsersQuery request, CancellationToken ct)
    {
        var users = await userFinder.GetAllAsync(ct);

        return ErrorOrFactory.From(users);
    }
}
