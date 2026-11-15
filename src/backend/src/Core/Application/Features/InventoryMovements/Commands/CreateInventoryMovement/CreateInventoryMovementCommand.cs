using Application.Features.InventoryMovements.DTOs;
using ErrorOr;
using MediatR;

namespace Application.Features.InventoryMovements.Commands.CreateInventoryMovement;

public sealed record CreateInventoryMovementCommand(CreateInventoryMovementRequest Request)
    : IRequest<ErrorOr<InventoryMovementResponse>>;
