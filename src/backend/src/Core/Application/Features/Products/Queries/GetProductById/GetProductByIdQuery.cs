using Application.Features.Products.DTOs;
using ErrorOr;
using MediatR;

namespace Application.Features.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid Id) : IRequest<ErrorOr<ProductResponse>>;
