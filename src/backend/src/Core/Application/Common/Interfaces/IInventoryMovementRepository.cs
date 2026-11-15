using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IInventoryMovementRepository
{
    Task<bool> AddAsync(InventoryMovement movement, CancellationToken ct = default);
}
