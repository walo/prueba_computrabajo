using Application.Common.Interfaces;
using Application.Features.Products.Common;
using Application.Features.Products.DTOs;
using Domain.Entities;
using Domain.Primitives;
using Domain.ValueObjects;
using ErrorOr;
using MediatR;

namespace Application.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateProductCommand, ErrorOr<ProductResponse>>
{
    public async Task<ErrorOr<ProductResponse>> Handle(UpdateProductCommand command, CancellationToken ct)
    {
        var product = await productRepository.GetByIdAsync(command.Id, ct);
        if (product is null)
        {
            return Error.NotFound("Product.NotFound", "El producto solicitado no existe.");
        }
        var categoryResult = await ProductWriteSupport.ResolveActiveCategoryAsync(
            categoryRepository,
            command.Request.CategoryId,
            ct);
        if (categoryResult.IsError)
        {
            return categoryResult.Errors;
        }
        var skuConflict = await ValidateSkuUniquenessAsync(command, ct);
        if (skuConflict is not null)
        {
            return skuConflict.Value;
        }

        return await ApplyAndSaveAsync(product, categoryResult.Value, command.Request, ct);
    }

    private async Task<Error?> ValidateSkuUniquenessAsync(UpdateProductCommand command, CancellationToken ct)
    {
        var normalizedSku = command.Request.Sku.Trim().ToUpperInvariant();
        var existingBySku = await productRepository.GetBySkuAsync(normalizedSku, ct);
        if (existingBySku is not null && existingBySku.Id != command.Id)
        {
            return Error.Conflict("Product.DuplicateSku", $"Ya existe otro producto con el SKU '{normalizedSku}'.");
        }

        return null;
    }

    private async Task<ErrorOr<ProductResponse>> ApplyAndSaveAsync(
        Product product,
        Category category,
        UpdateProductRequest request,
        CancellationToken ct)
    {
        var updateData = new ProductUpdateData(
            request.Sku,
            request.Name,
            request.Description,
            request.CategoryId,
            request.Price,
            request.MinStock,
            request.IsActive);
        var updateResult = product.Update(updateData);
        if (updateResult.IsError)
        {
            return updateResult.Errors;
        }
        await productRepository.UpdateAsync(product, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return ProductWriteSupport.MapToResponse(product, category);
    }
}
