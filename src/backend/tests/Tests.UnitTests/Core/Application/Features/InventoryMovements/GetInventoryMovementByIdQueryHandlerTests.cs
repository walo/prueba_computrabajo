using Application.Common.Interfaces;
using Application.Features.InventoryMovements.DTOs;
using Application.Features.InventoryMovements.Queries.GetInventoryMovementById;
using Domain.Constants;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace Tests.UnitTests.Core.Application.Features.InventoryMovements;

public sealed class GetInventoryMovementByIdQueryHandlerTests
{
    private readonly Mock<IInventoryMovementFinder> _finder = new();

    [Fact]
    public async Task GetById_Should_ReturnMovement_When_Exists()
    {
        var movementId = Guid.NewGuid();
        var expected = new InventoryMovementResponse(
            movementId,
            Guid.NewGuid(),
            "LAP-01",
            "Laptop",
            InventoryMovementTypes.Entry,
            5,
            10,
            15,
            "Restock",
            Guid.NewGuid(),
            "admin@enterprise.local",
            DateTime.UtcNow);
        _finder.Setup(f => f.GetByIdAsync(movementId, It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var handler = new GetInventoryMovementByIdQueryHandler(_finder.Object);
        var result = await handler.Handle(new GetInventoryMovementByIdQuery(movementId), CancellationToken.None);
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetById_Should_ReturnNotFound_When_MovementDoesNotExist()
    {
        var movementId = Guid.NewGuid();
        _finder.Setup(f => f.GetByIdAsync(movementId, It.IsAny<CancellationToken>())).ReturnsAsync((InventoryMovementResponse?)null);
        var handler = new GetInventoryMovementByIdQueryHandler(_finder.Object);
        var result = await handler.Handle(new GetInventoryMovementByIdQuery(movementId), CancellationToken.None);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Code.Should().Be("InventoryMovement.NotFound");
    }
}
