using Application.Features.Products.DTOs;
using Domain.ValueObjects;
using ErrorOr;
using MediatR;

namespace Application.Features.Products.Queries.GetProducts;

public sealed record GetProductsQuery(ProductFilter Filter) : IRequest<ErrorOr<PagedProductsResponse>>;
