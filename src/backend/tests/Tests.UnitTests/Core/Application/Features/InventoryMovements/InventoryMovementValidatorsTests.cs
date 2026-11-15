using Application.Features.InventoryMovements.Commands.CreateInventoryMovement;
using Application.Features.InventoryMovements.DTOs;
using Application.Features.InventoryMovements.Queries.GetInventoryMovementById;
using Application.Features.InventoryMovements.Queries.GetInventoryMovements;
using Domain.ValueObjects;
using FluentAssertions;

namespace Tests.UnitTests.Core.Application.Features.InventoryMovements;

public sealed class InventoryMovementValidatorsTests
{
    [Fact]
    public void CreateValidator_Should_FailSafely_When_RequestIsNull()
    {
        var validator = new CreateInventoryMovementCommandValidator();
        var result = validator.Validate(new CreateInventoryMovementCommand(null!));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Request");
    }

    [Theory]
    [InlineData("", 1, "Restock supply")]
    [InlineData("TRANSFER", 1, "Restock supply")]
    [InlineData("ENTRY", 0, "Restock supply")]
    [InlineData("EXIT", 2, "ab")]
    public void CreateValidator_Should_Fail_When_FieldsAreInvalid(string type, int quantity, string reason)
    {
        var validator = new CreateInventoryMovementCommandValidator();
        var command = new CreateInventoryMovementCommand(
            new CreateInventoryMovementRequest(Guid.NewGuid(), type, quantity, reason));
        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateValidator_Should_Pass_When_CommandIsValid()
    {
        var validator = new CreateInventoryMovementCommandValidator();
        var command = new CreateInventoryMovementCommand(
            new CreateInventoryMovementRequest(Guid.NewGuid(), "entry", 4, "Monthly warehouse restock"));
        var result = validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GetPagedValidator_Should_FailSafely_When_FilterIsNullOrDateRangeInverted()
    {
        var validator = new GetInventoryMovementsQueryValidator();
        var nullResult = validator.Validate(new GetInventoryMovementsQuery(null!));
        var invertedDates = validator.Validate(new GetInventoryMovementsQuery(
            new InventoryMovementFilter(1, 10, null, null, null, DateTime.UtcNow, DateTime.UtcNow.AddDays(-1))));
        nullResult.IsValid.Should().BeFalse();
        invertedDates.IsValid.Should().BeFalse();
    }

    [Fact]
    public void GetByIdValidator_Should_Fail_When_IdIsEmpty()
    {
        var validator = new GetInventoryMovementByIdQueryValidator();
        var result = validator.Validate(new GetInventoryMovementByIdQuery(Guid.Empty));
        result.IsValid.Should().BeFalse();
    }
}
