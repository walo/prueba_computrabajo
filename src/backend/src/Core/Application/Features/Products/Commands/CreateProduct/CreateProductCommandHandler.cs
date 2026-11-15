using Application.Common.Interfaces;
using Application.Features.Products.Common;
using Application.Features.Products.DTOs;
using Domain.Entities;
using Domain.Primitives;
using Domain.ValueObjects;
using ErrorOr;
using MediatR;

namespace Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateProductCommand, ErrorOr<ProductResponse>>
{
    public async Task<ErrorOr<ProductResponse>> Handle(CreateProductCommand command, CancellationToken ct)
    {
        var categoryResult = await ProductWriteSupport.ResolveActiveCategoryAsync(
            categoryRepository,
            command.Request.CategoryId,
            ct);
        if (categoryResult.IsError)
        {
            return categoryResult.Errors;
        }
        var normalizedSku = command.Request.Sku.Trim().ToUpperInvariant();
        var existingBySku = await productRepository.GetBySkuAsync(normalizedSku, ct);
        if (existingBySku is not null)
        {
            return Error.Conflict("Product.DuplicateSku", $"Ya existe un producto con el SKU '{normalizedSku}'.");
        }

        return await PersistProductAsync(command.Request, categoryResult.Value, ct);
    }

    private async Task<ErrorOr<ProductResponse>> PersistProductAsync(
        CreateProductRequest request,
        Category category,
        CancellationToken ct)
    {
        var creationData = new ProductCreationData(
            request.Sku,
            request.Name,
            request.Description,
            request.CategoryId,
            request.Price,
            request.InitialStock,
            request.MinStock);
        var productResult = Product.Create(creationData);
        if (productResult.IsError)
        {
            return productResult.Errors;
        }
        await productRepository.AddAsync(productResult.Value, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return ProductWriteSupport.MapToResponse(productResult.Value, category);
    }
}
