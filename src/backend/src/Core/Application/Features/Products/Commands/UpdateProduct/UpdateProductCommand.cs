using Application.Features.Products.DTOs;
using ErrorOr;
using MediatR;

namespace Application.Features.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(Guid Id, UpdateProductRequest Request) : IRequest<ErrorOr<ProductResponse>>;
