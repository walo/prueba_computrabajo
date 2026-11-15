using Application.Common.Interfaces;
using Application.Features.Products.DTOs;
using Domain.Entities;
using ErrorOr;

namespace Application.Features.Products.Common;

internal static class ProductWriteSupport
{
    internal static async Task<ErrorOr<Category>> ResolveActiveCategoryAsync(
        ICategoryRepository categoryRepository,
        Guid categoryId,
        CancellationToken ct)
    {
        var category = await categoryRepository.GetByIdAsync(categoryId, ct);
        if (category is null)
        {
            return Error.NotFound("Product.CategoryNotFound", "La categoría seleccionada no existe.");
        }
        if (!category.IsActive)
        {
            return Error.Conflict("Product.CategoryInactive", "No se puede asociar el producto a una categoría inactiva.");
        }

        return category;
    }

    internal static ProductResponse MapToResponse(Product product, Category category)
    {
        return new ProductResponse(
            product.Id,
            product.Sku,
            product.Name,
            product.Description,
            product.CategoryId,
            category.Code,
            category.Name,
            product.Price,
            product.Stock,
            product.MinStock,
            product.IsLowStock(),
            product.IsActive,
            product.CreatedAtUtc,
            product.UpdatedAtUtc);
    }
}
