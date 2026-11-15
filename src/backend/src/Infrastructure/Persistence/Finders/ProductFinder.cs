using Application.Common.Interfaces;
using Application.Features.Products.DTOs;
using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Finders;

public sealed class ProductFinder(AppDbContext dbContext) : IProductFinder
{
    public async Task<PagedProductsResponse> GetPagedAsync(ProductFilter filter, CancellationToken ct = default)
    {
        var query = BuildFilteredQuery(filter);
        var totalCount = await query.CountAsync(ct);
        var skip = (filter.Page - 1) * filter.PageSize;
        var pagedProducts = query
            .OrderBy(product => product.Name)
            .Skip(skip)
            .Take(filter.PageSize);
        var items = await ProjectToResponses(pagedProducts).ToListAsync(ct);

        return new PagedProductsResponse(items, filter.Page, filter.PageSize, totalCount);
    }

    public async Task<ProductResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var query = dbContext.Products
            .AsNoTracking()
            .Where(product => product.Id == id);

        return await ProjectToResponses(query).FirstOrDefaultAsync(ct);
    }

    private IQueryable<Product> BuildFilteredQuery(ProductFilter filter)
    {
        var query = dbContext.Products.AsNoTracking();
        if (filter.CategoryId.HasValue)
        {
            query = query.Where(product => product.CategoryId == filter.CategoryId.Value);
        }
        if (filter.IsActive.HasValue)
        {
            query = query.Where(product => product.IsActive == filter.IsActive.Value);
        }
        if (filter.LowStockOnly == true)
        {
            query = query.Where(product => product.Stock <= product.MinStock);
        }

        return ApplySearchFilter(query, filter.Search);
    }

    private static IQueryable<Product> ApplySearchFilter(IQueryable<Product> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }
        var term = search.Trim().ToLowerInvariant();

        return query.Where(product =>
            product.Sku.ToLower().Contains(term) ||
            product.Name.ToLower().Contains(term));
    }

    private IQueryable<ProductResponse> ProjectToResponses(IQueryable<Product> products)
    {
        return from product in products
               join category in dbContext.Categories.AsNoTracking()
                   on product.CategoryId equals category.Id into categoryGroup
               from category in categoryGroup.DefaultIfEmpty()
               select new ProductResponse(
                   product.Id,
                   product.Sku,
                   product.Name,
                   product.Description,
                   product.CategoryId,
                   category != null ? category.Code : string.Empty,
                   category != null ? category.Name : string.Empty,
                   product.Price,
                   product.Stock,
                   product.MinStock,
                   product.Stock <= product.MinStock,
                   product.IsActive,
                   product.CreatedAtUtc,
                   product.UpdatedAtUtc);
    }
}
