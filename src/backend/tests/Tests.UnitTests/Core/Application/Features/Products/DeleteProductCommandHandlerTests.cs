using Application.Common.Interfaces;
using Application.Features.Products.Commands.DeleteProduct;
using Domain.Entities;
using Domain.Primitives;
using Domain.ValueObjects;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace Tests.UnitTests.Core.Application.Features.Products;

public sealed class DeleteProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task Handle_Should_DeleteProduct_When_StockIsZeroAndHasNoMovements()
    {
        var product = Product.Create(new ProductCreationData("SKU-1", "Mouse", null, Guid.NewGuid(), 10m, 0, 2)).Value;
        _productRepository.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _productRepository.Setup(r => r.HasInventoryMovementsAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var handler = new DeleteProductCommandHandler(_productRepository.Object, _unitOfWork.Object);
        var result = await handler.Handle(new DeleteProductCommand(product.Id), CancellationToken.None);
        result.IsError.Should().BeFalse();
        _productRepository.Verify(r => r.DeleteAsync(product, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnConflict_When_StockIsGreaterThanZero()
    {
        var product = Product.Create(new ProductCreationData("SKU-1", "Mouse", null, Guid.NewGuid(), 10m, 5, 2)).Value;
        _productRepository.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        var handler = new DeleteProductCommandHandler(_productRepository.Object, _unitOfWork.Object);
        var result = await handler.Handle(new DeleteProductCommand(product.Id), CancellationToken.None);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
        result.FirstError.Code.Should().Be("Product.CannotDeleteWithStockOrMovements");
    }

    [Fact]
    public async Task Handle_Should_ReturnConflict_When_ProductHasInventoryMovements()
    {
        var product = Product.Create(new ProductCreationData("SKU-1", "Mouse", null, Guid.NewGuid(), 10m, 0, 2)).Value;
        _productRepository.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _productRepository.Setup(r => r.HasInventoryMovementsAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = new DeleteProductCommandHandler(_productRepository.Object, _unitOfWork.Object);
        var result = await handler.Handle(new DeleteProductCommand(product.Id), CancellationToken.None);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
        result.FirstError.Code.Should().Be("Product.CannotDeleteWithStockOrMovements");
    }
}
