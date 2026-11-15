using Application.Common.Interfaces;
using Application.Features.InventoryMovements.DTOs;
using Application.Features.InventoryMovements.Queries.GetInventoryMovements;
using Domain.Constants;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Tests.UnitTests.Core.Application.Features.InventoryMovements;

public sealed class GetInventoryMovementsQueryHandlerTests
{
    private readonly Mock<IInventoryMovementFinder> _finder = new();

    [Fact]
    public async Task GetPaged_Should_DelegateToFinderAndReturnPagedResponse()
    {
        var filter = new InventoryMovementFilter(1, 10, "lap", null, InventoryMovementTypes.Entry);
        var item = new InventoryMovementResponse(
            Guid.NewGuid(),
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
        var expected = new PagedInventoryMovementsResponse([item], 1, 10, 1);
        _finder.Setup(f => f.GetPagedAsync(filter, It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var handler = new GetInventoryMovementsQueryHandler(_finder.Object);
        var result = await handler.Handle(new GetInventoryMovementsQuery(filter), CancellationToken.None);
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(expected);
    }
}
