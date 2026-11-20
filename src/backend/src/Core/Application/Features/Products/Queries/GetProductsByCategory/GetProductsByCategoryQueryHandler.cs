using Application.Common.Interfaces;
using Application.Features.Products.DTOs;
using Domain.ValueObjects;
using ErrorOr;
using MediatR;

namespace Application.Features.Products.Queries.GetProductsByCategory;

public sealed class GetProductsByCategoryQueryHandler(
    ICategoryFinder categoryFinder,
    IProductFinder productFinder)
    : IRequestHandler<GetProductsByCategoryQuery, ErrorOr<PagedProductsResponse>>
{
    public async Task<ErrorOr<PagedProductsResponse>> Handle(
        GetProductsByCategoryQuery query,
        CancellationToken ct)
    {
        var categoryExists = await categoryFinder.ExistsAsync(query.Request.CategoryId, ct);
        if (!categoryExists)
        {
            return Error.NotFound("Category.NotFound", "La categoría solicitada no existe.");
        }
        var filter = BuildProductFilter(query.Request);

        return await productFinder.GetPagedAsync(filter, ct);
    }

    private static ProductFilter BuildProductFilter(CategoryProductsQuery request)
    {
        return new ProductFilter(
            request.Filter.Page,
            request.Filter.PageSize,
            request.Filter.Search,
            request.CategoryId,
            request.Filter.IsActive,
            request.Filter.LowStockOnly);
    }
}
