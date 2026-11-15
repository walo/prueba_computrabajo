using Application.Common.Interfaces;
using Application.Features.Products.Commands.UpdateProduct;
using Application.Features.Products.DTOs;
using Domain.Entities;
using Domain.Primitives;
using Domain.ValueObjects;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace Tests.UnitTests.Core.Application.Features.Products;

public sealed class UpdateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly Mock<ICategoryRepository> _categoryRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task Handle_Should_UpdateProduct_When_ValidAndUniqueSku()
    {
        var category = Category.Create(new CategoryCreationData("ELEC", "Electronics", null)).Value;
        var existing = Product.Create(new ProductCreationData("SKU-1", "Mouse", null, category.Id, 15m, 12, 3)).Value;
        _productRepository.Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _categoryRepository.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        _productRepository.Setup(r => r.GetBySkuAsync("SKU-2", It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);
        var handler = new UpdateProductCommandHandler(_productRepository.Object, _categoryRepository.Object, _unitOfWork.Object);
        var result = await handler.Handle(
            new UpdateProductCommand(existing.Id, new UpdateProductRequest("sku-2", "Mouse Pro", "Optical", category.Id, 35m, 15, false)),
            CancellationToken.None);
        result.IsError.Should().BeFalse();
        result.Value.Sku.Should().Be("SKU-2");
        result.Value.Stock.Should().Be(12);
        result.Value.IsLowStock.Should().BeTrue();
        result.Value.IsActive.Should().BeFalse();
        _productRepository.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_ProductDoesNotExist()
    {
        var id = Guid.NewGuid();
        _productRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);
        var handler = new UpdateProductCommandHandler(_productRepository.Object, _categoryRepository.Object, _unitOfWork.Object);
        var result = await handler.Handle(
            new UpdateProductCommand(id, new UpdateProductRequest("SKU-1", "Mouse", null, Guid.NewGuid(), 10m, 1, true)),
            CancellationToken.None);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Code.Should().Be("Product.NotFound");
    }

    [Fact]
    public async Task Handle_Should_ReturnConflict_When_AnotherProductUsesSku()
    {
        var category = Category.Create(new CategoryCreationData("ELEC", "Electronics", null)).Value;
        var target = Product.Create(new ProductCreationData("SKU-1", "Mouse", null, category.Id, 15m, 5, 1)).Value;
        var conflicting = Product.Create(new ProductCreationData("SKU-TAKEN", "Keyboard", null, category.Id, 25m, 5, 1)).Value;
        _productRepository.Setup(r => r.GetByIdAsync(target.Id, It.IsAny<CancellationToken>())).ReturnsAsync(target);
        _categoryRepository.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        _productRepository.Setup(r => r.GetBySkuAsync("SKU-TAKEN", It.IsAny<CancellationToken>())).ReturnsAsync(conflicting);
        var handler = new UpdateProductCommandHandler(_productRepository.Object, _categoryRepository.Object, _unitOfWork.Object);
        var result = await handler.Handle(
            new UpdateProductCommand(target.Id, new UpdateProductRequest("sku-taken", "Mouse", null, category.Id, 15m, 1, true)),
            CancellationToken.None);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
        result.FirstError.Code.Should().Be("Product.DuplicateSku");
    }
}
