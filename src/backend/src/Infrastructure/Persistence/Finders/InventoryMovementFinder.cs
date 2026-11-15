using Application.Common.Interfaces;
using Application.Features.InventoryMovements.DTOs;
using Domain.Constants;
using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Finders;

public sealed class InventoryMovementFinder(AppDbContext dbContext) : IInventoryMovementFinder
{
    public async Task<PagedInventoryMovementsResponse> GetPagedAsync(
        InventoryMovementFilter filter,
        CancellationToken ct = default)
    {
        var baseQuery = BuildFilteredMovements(filter);
        var totalCount = await baseQuery.CountAsync(ct);
        var skip = (filter.Page - 1) * filter.PageSize;
        var movements = await baseQuery
            .OrderByDescending(movement => movement.CreatedAtUtc)
            .Skip(skip)
            .Take(filter.PageSize)
            .ToListAsync(ct);
        var items = await MapMovementsAsync(movements, ct);

        return new PagedInventoryMovementsResponse(items, filter.Page, filter.PageSize, totalCount);
    }

    public async Task<InventoryMovementResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var movement = await dbContext.InventoryMovements
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, ct);
        if (movement is null)
        {
            return null;
        }
        var items = await MapMovementsAsync([movement], ct);

        return items[0];
    }

    private IQueryable<InventoryMovement> BuildFilteredMovements(InventoryMovementFilter filter)
    {
        var query = ApplyEntityFilters(dbContext.InventoryMovements.AsNoTracking(), filter);
        query = ApplyDateFilters(query, filter);

        return ApplySearchFilter(query, filter.Search);
    }

    private static IQueryable<InventoryMovement> ApplyEntityFilters(
        IQueryable<InventoryMovement> query,
        InventoryMovementFilter filter)
    {
        if (filter.ProductId.HasValue)
        {
            query = query.Where(movement => movement.ProductId == filter.ProductId.Value);
        }
        if (!string.IsNullOrWhiteSpace(filter.Type))
        {
            var normalizedType = InventoryMovementTypes.Normalize(filter.Type);
            query = query.Where(movement => movement.Type == normalizedType);
        }

        return query;
    }

    private static IQueryable<InventoryMovement> ApplyDateFilters(
        IQueryable<InventoryMovement> query,
        InventoryMovementFilter filter)
    {
        if (filter.FromDateUtc.HasValue)
        {
            query = query.Where(movement => movement.CreatedAtUtc >= filter.FromDateUtc.Value);
        }
        if (filter.ToDateUtc.HasValue)
        {
            query = query.Where(movement => movement.CreatedAtUtc <= filter.ToDateUtc.Value);
        }

        return query;
    }

    private IQueryable<InventoryMovement> ApplySearchFilter(
        IQueryable<InventoryMovement> query,
        string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }
        var term = search.Trim().ToLowerInvariant();
        var matchingProductIds = dbContext.Products
            .AsNoTracking()
            .Where(product =>
                product.Sku.ToLower().Contains(term) ||
                product.Name.ToLower().Contains(term))
            .Select(product => product.Id);

        return query.Where(movement =>
            movement.Reason.ToLower().Contains(term) ||
            matchingProductIds.Contains(movement.ProductId));
    }

    private async Task<IReadOnlyList<InventoryMovementResponse>> MapMovementsAsync(
        IReadOnlyList<InventoryMovement> movements,
        CancellationToken ct)
    {
        if (movements.Count == 0)
        {
            return [];
        }
        var productIds = movements.Select(movement => movement.ProductId).Distinct().ToList();
        var userIds = movements.Select(movement => movement.PerformedByUserId).Distinct().ToList();
        var products = await LoadProductLookupAsync(productIds, ct);
        var users = await LoadUserLookupAsync(userIds, ct);

        return movements
            .Select(movement => MapSingleMovement(movement, products, users))
            .ToList();
    }

    private async Task<Dictionary<Guid, (string Sku, string Name)>> LoadProductLookupAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken ct)
    {
        var rows = await dbContext.Products
            .AsNoTracking()
            .Where(product => productIds.Contains(product.Id))
            .Select(product => new { product.Id, product.Sku, product.Name })
            .ToListAsync(ct);

        return rows.ToDictionary(row => row.Id, row => (row.Sku, row.Name));
    }

    private async Task<Dictionary<Guid, string>> LoadUserLookupAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken ct)
    {
        var rows = await dbContext.Users
            .AsNoTracking()
            .Where(user => userIds.Contains(user.Id))
            .Select(user => new { user.Id, user.Email })
            .ToListAsync(ct);

        return rows.ToDictionary(row => row.Id, row => row.Email);
    }

    private static InventoryMovementResponse MapSingleMovement(
        InventoryMovement movement,
        IReadOnlyDictionary<Guid, (string Sku, string Name)> products,
        IReadOnlyDictionary<Guid, string> users)
    {
        var hasProduct = products.TryGetValue(movement.ProductId, out var productInfo);
        var hasUser = users.TryGetValue(movement.PerformedByUserId, out var userEmail);
        var sku = hasProduct ? productInfo.Sku : string.Empty;
        var name = hasProduct ? productInfo.Name : string.Empty;
        var email = hasUser && userEmail is not null ? userEmail : string.Empty;

        return new InventoryMovementResponse(
            movement.Id,
            movement.ProductId,
            sku,
            name,
            movement.Type,
            movement.Quantity,
            movement.PreviousStock,
            movement.NewStock,
            movement.Reason,
            movement.PerformedByUserId,
            email,
            movement.CreatedAtUtc);
    }
}
