using Application.Features.Products.DTOs;
using ErrorOr;
using MediatR;

namespace Application.Features.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(CreateProductRequest Request) : IRequest<ErrorOr<ProductResponse>>;
