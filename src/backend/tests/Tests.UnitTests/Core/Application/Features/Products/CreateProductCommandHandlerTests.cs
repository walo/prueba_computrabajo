using Application.Common.Interfaces;
using Application.Features.Products.Commands.CreateProduct;
using Application.Features.Products.DTOs;
using Domain.Entities;
using Domain.Primitives;
using Domain.ValueObjects;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace Tests.UnitTests.Core.Application.Features.Products;

public sealed class CreateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly Mock<ICategoryRepository> _categoryRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task Handle_Should_CreateProduct_When_CategoryIsActiveAndSkuIsUnique()
    {
        var category = Category.Create(new CategoryCreationData("ELEC", "Electronics", null)).Value;
        _categoryRepository.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        _productRepository.Setup(r => r.GetBySkuAsync("LAP-01", It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);
        var handler = new CreateProductCommandHandler(_productRepository.Object, _categoryRepository.Object, _unitOfWork.Object);
        var request = new CreateProductRequest(" lap-01 ", "Laptop 14", "Workstation", category.Id, 999.50m, 8, 2);
        var result = await handler.Handle(new CreateProductCommand(request), CancellationToken.None);
        result.IsError.Should().BeFalse();
        result.Value.Sku.Should().Be("LAP-01");
        result.Value.CategoryCode.Should().Be("elec");
        result.Value.CategoryName.Should().Be("Electronics");
        result.Value.Stock.Should().Be(8);
        _productRepository.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_CategoryDoesNotExist()
    {
        var categoryId = Guid.NewGuid();
        _categoryRepository.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);
        var handler = new CreateProductCommandHandler(_productRepository.Object, _categoryRepository.Object, _unitOfWork.Object);
        var result = await handler.Handle(
            new CreateProductCommand(new CreateProductRequest("SKU-1", "Item", null, categoryId, 10m, 1, 0)),
            CancellationToken.None);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Code.Should().Be("Product.CategoryNotFound");
    }

    [Fact]
    public async Task Handle_Should_ReturnConflict_When_CategoryIsInactive()
    {
        var category = Category.Create(new CategoryCreationData("OLD", "Obsolete", null)).Value;
        category.Update(new CategoryUpdateData("OLD", "Obsolete", null, false));
        _categoryRepository.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        var handler = new CreateProductCommandHandler(_productRepository.Object, _categoryRepository.Object, _unitOfWork.Object);
        var result = await handler.Handle(
            new CreateProductCommand(new CreateProductRequest("SKU-1", "Item", null, category.Id, 10m, 1, 0)),
            CancellationToken.None);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
        result.FirstError.Code.Should().Be("Product.CategoryInactive");
    }

    [Fact]
    public async Task Handle_Should_ReturnConflict_When_SkuAlreadyExists()
    {
        var category = Category.Create(new CategoryCreationData("ELEC", "Electronics", null)).Value;
        var existing = Product.Create(new ProductCreationData("SKU-DUP", "Existing", null, category.Id, 20m, 2, 1)).Value;
        _categoryRepository.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        _productRepository.Setup(r => r.GetBySkuAsync("SKU-DUP", It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        var handler = new CreateProductCommandHandler(_productRepository.Object, _categoryRepository.Object, _unitOfWork.Object);
        var result = await handler.Handle(
            new CreateProductCommand(new CreateProductRequest("sku-dup", "Another", null, category.Id, 30m, 1, 0)),
            CancellationToken.None);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
        result.FirstError.Code.Should().Be("Product.DuplicateSku");
    }
}
