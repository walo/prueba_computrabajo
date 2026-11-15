using Application.Common.Interfaces;
using Application.Features.Products.DTOs;
using ErrorOr;
using MediatR;

namespace Application.Features.Products.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler(IProductFinder productFinder)
    : IRequestHandler<GetProductByIdQuery, ErrorOr<ProductResponse>>
{
    public async Task<ErrorOr<ProductResponse>> Handle(GetProductByIdQuery query, CancellationToken ct)
    {
        var product = await productFinder.GetByIdAsync(query.Id, ct);
        if (product is null)
        {
            return Error.NotFound("Product.NotFound", "El producto solicitado no existe.");
        }

        return product;
    }
}
