using Application.Features.Users.DTOs;
using Application.Features.Users.Queries.GetUsers;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.API.Attributes;

namespace Web.API.Controllers;

[Route("api/v1/users")]
public sealed class UsersController(ISender sender) : ApiController
{
    [HttpGet]
    [HasPermission(Permissions.UsersRead)]
    [ProducesResponseType(typeof(IReadOnlyList<UserSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await sender.Send(new GetUsersQuery(), ct);

        return result.Match(Ok, HandleFailure);
    }
}
