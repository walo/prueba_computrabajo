using Application.Common.Interfaces;
using Application.Features.Categories.DTOs;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Finders;

public sealed class CategoryFinder(AppDbContext dbContext) : ICategoryFinder
{
    public async Task<PagedCategoriesResponse> GetPagedAsync(CategoryFilter filter, CancellationToken ct = default)
    {
        var query = BuildFilteredQuery(filter);
        var totalCount = await query.CountAsync(ct);
        var skip = (filter.Page - 1) * filter.PageSize;
        var items = await query
            .OrderBy(category => category.Name)
            .Skip(skip)
            .Take(filter.PageSize)
            .Select(category => new CategoryResponse(
                category.Id,
                category.Code,
                category.Name,
                category.Description,
                category.IsActive,
                category.CreatedAtUtc,
                category.UpdatedAtUtc))
            .ToListAsync(ct);

        return new PagedCategoriesResponse(items, filter.Page, filter.PageSize, totalCount);
    }

    public async Task<CategoryResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .Where(category => category.Id == id)
            .Select(category => new CategoryResponse(
                category.Id,
                category.Code,
                category.Name,
                category.Description,
                category.IsActive,
                category.CreatedAtUtc,
                category.UpdatedAtUtc))
            .FirstOrDefaultAsync(ct);
    }

    private IQueryable<Category> BuildFilteredQuery(CategoryFilter filter)
    {
        var query = dbContext.Categories.AsNoTracking();
        if (filter.IsActive.HasValue)
        {
            query = query.Where(category => category.IsActive == filter.IsActive.Value);
        }
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLowerInvariant();
            query = query.Where(category =>
                category.Code.ToLower().Contains(term) ||
                category.Name.ToLower().Contains(term));
        }

        return query;
    }
}
