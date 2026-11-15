using Application.Features.InventoryMovements.Commands.CreateInventoryMovement;
using Application.Features.InventoryMovements.DTOs;
using Application.Features.InventoryMovements.Queries.GetInventoryMovementById;
using Application.Features.InventoryMovements.Queries.GetInventoryMovements;
using Domain.Constants;
using Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.API.Attributes;

namespace Web.API.Controllers;

[Route("api/v1/inventory-movements")]
public sealed class InventoryMovementsController(ISender sender) : ApiController
{
    [HttpGet]
    [HasPermission(Permissions.InventoryMovements.Read)]
    [ProducesResponseType(typeof(PagedInventoryMovementsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll([FromQuery] InventoryMovementFilter filter, CancellationToken ct)
    {
        var result = await sender.Send(new GetInventoryMovementsQuery(filter), ct);

        return result.Match(Ok, HandleFailure);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.InventoryMovements.Read)]
    [ProducesResponseType(typeof(InventoryMovementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetInventoryMovementByIdQuery(id), ct);

        return result.Match(Ok, HandleFailure);
    }

    [HttpPost]
    [HasPermission(Permissions.InventoryMovements.Create)]
    [ProducesResponseType(typeof(InventoryMovementResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateInventoryMovementRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CreateInventoryMovementCommand(request), ct);

        return result.Match(
            movement => CreatedAtAction(nameof(GetById), new { id = movement.Id }, movement),
            HandleFailure);
    }
}
