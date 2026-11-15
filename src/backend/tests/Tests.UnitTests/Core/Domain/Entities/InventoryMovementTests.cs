using Domain.Constants;
using Domain.Entities;
using Domain.Events;
using Domain.ValueObjects;
using ErrorOr;
using FluentAssertions;

namespace Tests.UnitTests.Core.Domain.Entities;

public sealed class InventoryMovementTests
{
    [Fact]
    public void Create_Should_IncreaseStockAndNormalizeType_When_EntryIsValid()
    {
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var data = new InventoryMovementCreationData(productId, "  entry ", 5, 10, "  Restock supplier #12  ", userId);
        var result = InventoryMovement.Create(data);
        result.IsError.Should().BeFalse();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.ProductId.Should().Be(productId);
        result.Value.Type.Should().Be(InventoryMovementTypes.Entry);
        result.Value.Quantity.Should().Be(5);
        result.Value.PreviousStock.Should().Be(10);
        result.Value.NewStock.Should().Be(15);
        result.Value.Reason.Should().Be("Restock supplier #12");
        result.Value.PerformedByUserId.Should().Be(userId);
        result.Value.DomainEvents.Should().ContainSingle(e => e is InventoryMovementCreatedDomainEvent);
    }

    [Fact]
    public void Create_Should_DecreaseStock_When_ExitIsWithinAvailableStock()
    {
        var data = new InventoryMovementCreationData(Guid.NewGuid(), "exit", 8, 10, "Sales order dispatch", Guid.NewGuid());
        var result = InventoryMovement.Create(data);
        result.IsError.Should().BeFalse();
        result.Value.Type.Should().Be(InventoryMovementTypes.Exit);
        result.Value.PreviousStock.Should().Be(10);
        result.Value.NewStock.Should().Be(2);
    }

    [Fact]
    public void Create_Should_ReturnConflict_When_ExitExceedsPreviousStock()
    {
        var data = new InventoryMovementCreationData(Guid.NewGuid(), InventoryMovementTypes.Exit, 11, 10, "Excess dispatch", Guid.NewGuid());
        var result = InventoryMovement.Create(data);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
        result.FirstError.Code.Should().Be("InventoryMovement.InsufficientStock");
    }

    [Theory]
    [InlineData("")]
    [InlineData("ADJUST")]
    public void Create_Should_ReturnValidationError_When_TypeIsInvalid(string invalidType)
    {
        var data = new InventoryMovementCreationData(Guid.NewGuid(), invalidType, 1, 5, "Valid reason", Guid.NewGuid());
        var result = InventoryMovement.Create(data);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("InventoryMovement.InvalidType");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Create_Should_ReturnValidationError_When_QuantityIsNotPositive(int invalidQuantity)
    {
        var data = new InventoryMovementCreationData(Guid.NewGuid(), InventoryMovementTypes.Entry, invalidQuantity, 5, "Valid reason", Guid.NewGuid());
        var result = InventoryMovement.Create(data);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("InventoryMovement.InvalidQuantity");
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    public void Create_Should_ReturnValidationError_When_ReasonIsInvalid(string invalidReason)
    {
        var data = new InventoryMovementCreationData(Guid.NewGuid(), InventoryMovementTypes.Entry, 2, 5, invalidReason, Guid.NewGuid());
        var result = InventoryMovement.Create(data);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("InventoryMovement.InvalidReason");
    }

    [Fact]
    public void Create_Should_ReturnValidationError_When_DataIsNull()
    {
        var result = InventoryMovement.Create(null!);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("InventoryMovement.InvalidData");
    }
}
