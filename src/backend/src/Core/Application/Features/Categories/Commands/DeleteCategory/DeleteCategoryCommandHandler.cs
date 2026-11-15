using Application.Common.Interfaces;
using Domain.Primitives;
using ErrorOr;
using MediatR;

namespace Application.Features.Categories.Commands.DeleteCategory;

public sealed class DeleteCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteCategoryCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(DeleteCategoryCommand command, CancellationToken ct)
    {
        var category = await categoryRepository.GetByIdAsync(command.Id, ct);
        if (category is null)
        {
            return Error.NotFound("Category.NotFound", "La categoría solicitada no existe.");
        }
        var hasProducts = await categoryRepository.HasAssociatedProductsAsync(category.Id, ct);
        if (hasProducts)
        {
            return Error.Conflict(
                "Category.HasAssociatedProducts",
                "No se puede eliminar la categoría porque tiene productos asociados. Considere inactivarla.");
        }
        category.MarkDeleted();
        await categoryRepository.DeleteAsync(category, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Deleted;
    }
}
