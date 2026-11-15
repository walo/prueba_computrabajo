using Application.Common.Interfaces;
using Application.Features.InventoryMovements.DTOs;
using ErrorOr;
using MediatR;

namespace Application.Features.InventoryMovements.Queries.GetInventoryMovementById;

public sealed class GetInventoryMovementByIdQueryHandler(IInventoryMovementFinder finder)
    : IRequestHandler<GetInventoryMovementByIdQuery, ErrorOr<InventoryMovementResponse>>
{
    public async Task<ErrorOr<InventoryMovementResponse>> Handle(
        GetInventoryMovementByIdQuery query,
        CancellationToken ct)
    {
        var movement = await finder.GetByIdAsync(query.Id, ct);
        if (movement is null)
        {
            return Error.NotFound("InventoryMovement.NotFound", "El movimiento de inventario solicitado no existe.");
        }

        return movement;
    }
}
