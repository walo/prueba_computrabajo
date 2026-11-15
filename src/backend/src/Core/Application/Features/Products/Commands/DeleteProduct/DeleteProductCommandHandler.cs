using Application.Common.Interfaces;
using Domain.Primitives;
using ErrorOr;
using MediatR;

namespace Application.Features.Products.Commands.DeleteProduct;

public sealed class DeleteProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteProductCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(DeleteProductCommand command, CancellationToken ct)
    {
        var product = await productRepository.GetByIdAsync(command.Id, ct);
        if (product is null)
        {
            return Error.NotFound("Product.NotFound", "El producto solicitado no existe.");
        }
        var hasMovements = await productRepository.HasInventoryMovementsAsync(product.Id, ct);
        if (product.Stock > 0 || hasMovements)
        {
            return Error.Conflict(
                "Product.CannotDeleteWithStockOrMovements",
                "No se puede eliminar un producto con stock mayor a cero o movimientos históricos. Considere inactivarlo.");
        }
        product.MarkDeleted();
        await productRepository.DeleteAsync(product, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Deleted;
    }
}
