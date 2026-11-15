using Application.Features.InventoryMovements.DTOs;
using Domain.ValueObjects;
using ErrorOr;
using MediatR;

namespace Application.Features.InventoryMovements.Queries.GetInventoryMovements;

public sealed record GetInventoryMovementsQuery(InventoryMovementFilter Filter)
    : IRequest<ErrorOr<PagedInventoryMovementsResponse>>;
