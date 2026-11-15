using ErrorOr;
using MediatR;

namespace Application.Features.Products.Commands.DeleteProduct;

public sealed record DeleteProductCommand(Guid Id) : IRequest<ErrorOr<Deleted>>;
