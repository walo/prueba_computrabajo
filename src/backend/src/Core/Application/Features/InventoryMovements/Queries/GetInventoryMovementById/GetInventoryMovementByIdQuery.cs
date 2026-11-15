using Application.Features.InventoryMovements.DTOs;
using ErrorOr;
using MediatR;

namespace Application.Features.InventoryMovements.Queries.GetInventoryMovementById;

public sealed record GetInventoryMovementByIdQuery(Guid Id)
    : IRequest<ErrorOr<InventoryMovementResponse>>;
