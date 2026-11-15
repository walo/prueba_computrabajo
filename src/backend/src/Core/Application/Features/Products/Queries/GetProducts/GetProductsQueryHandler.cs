using Application.Common.Interfaces;
using Application.Features.Products.DTOs;
using ErrorOr;
using MediatR;

namespace Application.Features.Products.Queries.GetProducts;

public sealed class GetProductsQueryHandler(IProductFinder productFinder)
    : IRequestHandler<GetProductsQuery, ErrorOr<PagedProductsResponse>>
{
    public async Task<ErrorOr<PagedProductsResponse>> Handle(GetProductsQuery query, CancellationToken ct)
    {
        return await productFinder.GetPagedAsync(query.Filter, ct);
    }
}
