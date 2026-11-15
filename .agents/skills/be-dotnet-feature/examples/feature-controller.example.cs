using Application.Attributes;
using Application.Features.Items.Commands.CreateItem;
using Application.Features.Items.DTOs;
using Application.Features.Items.Queries.GetItems;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Web.API.Controllers;

[Authorize]
public sealed class ItemsController : ApiController
{
    [HttpGet]
    [RequirePermission(PermissionType.FeaturesRead)]
    [ProducesResponseType(typeof(List<ItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, CancellationToken cancellationToken)
    {
        var query = new GetItemsQuery(search);
        var result = await Mediator.Send(query, cancellationToken);

        return result.Match(
            items => Ok(items),
            errors => Problem(errors));
    }

    [HttpPost]
    [RequirePermission(PermissionType.FeaturesCreate)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateItemCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);

        return result.Match(
            id => CreatedAtAction(nameof(GetAll), new { id }, id),
            errors => Problem(errors));
    }
}
