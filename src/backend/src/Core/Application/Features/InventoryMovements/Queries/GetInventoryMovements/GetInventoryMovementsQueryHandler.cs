using Application.Common.Interfaces;
using Application.Features.InventoryMovements.DTOs;
using ErrorOr;
using MediatR;

namespace Application.Features.InventoryMovements.Queries.GetInventoryMovements;

public sealed class GetInventoryMovementsQueryHandler(IInventoryMovementFinder finder)
    : IRequestHandler<GetInventoryMovementsQuery, ErrorOr<PagedInventoryMovementsResponse>>
{
    public async Task<ErrorOr<PagedInventoryMovementsResponse>> Handle(
        GetInventoryMovementsQuery query,
        CancellationToken ct)
    {
        return await finder.GetPagedAsync(query.Filter, ct);
    }
}
