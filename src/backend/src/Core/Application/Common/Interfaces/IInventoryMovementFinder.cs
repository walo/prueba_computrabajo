using Application.Features.InventoryMovements.DTOs;
using Domain.ValueObjects;

namespace Application.Common.Interfaces;

public interface IInventoryMovementFinder
{
    Task<PagedInventoryMovementsResponse> GetPagedAsync(InventoryMovementFilter filter, CancellationToken ct = default);
    Task<InventoryMovementResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);
}
