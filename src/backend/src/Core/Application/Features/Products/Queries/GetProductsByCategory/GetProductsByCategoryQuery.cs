using Application.Features.Products.DTOs;
using Domain.ValueObjects;
using ErrorOr;
using MediatR;

namespace Application.Features.Products.Queries.GetProductsByCategory;

public sealed record GetProductsByCategoryQuery(CategoryProductsQuery Request)
    : IRequest<ErrorOr<PagedProductsResponse>>;
