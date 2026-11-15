using Application.Features.Products.DTOs;
using Domain.ValueObjects;

namespace Application.Common.Interfaces;

public interface IProductFinder
{
    Task<PagedProductsResponse> GetPagedAsync(ProductFilter filter, CancellationToken ct = default);
    Task<ProductResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);
}
