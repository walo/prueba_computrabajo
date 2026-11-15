using Domain.Entities;
using Domain.Events;
using Domain.ValueObjects;
using ErrorOr;
using FluentAssertions;

namespace Tests.UnitTests.Core.Domain.Entities;

public sealed class ProductTests
{
    [Fact]
    public void Create_Should_NormalizeSkuAndSetInitialState_When_ValidDataProvided()
    {
        var categoryId = Guid.NewGuid();
        var data = new ProductCreationData("  lap-001 ", "  Laptop Pro 14  ", "  Workstation  ", categoryId, 1299.99m, 10, 3);
        var result = Product.Create(data);
        result.IsError.Should().BeFalse();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.Sku.Should().Be("LAP-001");
        result.Value.Name.Should().Be("Laptop Pro 14");
        result.Value.Description.Should().Be("Workstation");
        result.Value.CategoryId.Should().Be(categoryId);
        result.Value.Price.Should().Be(1299.99m);
        result.Value.Stock.Should().Be(10);
        result.Value.MinStock.Should().Be(3);
        result.Value.IsActive.Should().BeTrue();
        result.Value.IsLowStock().Should().BeFalse();
        result.Value.DomainEvents.Should().ContainSingle(e => e is ProductCreatedDomainEvent);
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    [InlineData("SKU WITH SPACE")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ12345")]
    public void Create_Should_ReturnValidationError_When_SkuIsInvalid(string invalidSku)
    {
        var data = new ProductCreationData(invalidSku, "Valid Product", null, Guid.NewGuid(), 10m, 5, 2);
        var result = Product.Create(data);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("Product.InvalidSku");
    }

    [Fact]
    public void Create_Should_ReturnValidationError_When_PriceOrStockIsNegative()
    {
        var negativePrice = Product.Create(new ProductCreationData("SKU-1", "Valid", null, Guid.NewGuid(), -1m, 5, 2));
        var negativeStock = Product.Create(new ProductCreationData("SKU-2", "Valid", null, Guid.NewGuid(), 10m, -1, 2));
        negativePrice.FirstError.Code.Should().Be("Product.InvalidPrice");
        negativeStock.FirstError.Code.Should().Be("Product.InvalidInitialStock");
    }

    [Fact]
    public void Update_Should_PreserveStockAndEmitEvent_When_ValidDataProvided()
    {
        var initialCategory = Guid.NewGuid();
        var updatedCategory = Guid.NewGuid();
        var product = Product.Create(new ProductCreationData("SKU-10", "Mouse", null, initialCategory, 25m, 15, 5)).Value;
        var updateResult = product.Update(new ProductUpdateData(" sku-20 ", " Gaming Mouse ", "RGB", updatedCategory, 45m, 20, false));
        updateResult.IsError.Should().BeFalse();
        product.Sku.Should().Be("SKU-20");
        product.Name.Should().Be("Gaming Mouse");
        product.CategoryId.Should().Be(updatedCategory);
        product.Price.Should().Be(45m);
        product.Stock.Should().Be(15);
        product.MinStock.Should().Be(20);
        product.IsActive.Should().BeFalse();
        product.IsLowStock().Should().BeTrue();
        product.UpdatedAtUtc.Should().NotBeNull();
        product.DomainEvents.Should().ContainSingle(e => e is ProductUpdatedDomainEvent);
    }

    [Fact]
    public void AdjustStock_Should_UpdateStockOrFail_When_ResultWouldBeNegative()
    {
        var product = Product.Create(new ProductCreationData("SKU-30", "Keyboard", null, Guid.NewGuid(), 50m, 4, 2)).Value;
        var invalidAdjust = product.AdjustStock(-1);
        var validAdjust = product.AdjustStock(2);
        invalidAdjust.IsError.Should().BeTrue();
        invalidAdjust.FirstError.Code.Should().Be("Product.InsufficientStock");
        validAdjust.IsError.Should().BeFalse();
        product.Stock.Should().Be(2);
        product.IsLowStock().Should().BeTrue();
    }
}
