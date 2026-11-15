using Domain.Entities;
using Domain.Events;
using ErrorOr;
using FluentAssertions;

namespace Tests.UnitTests.Core.Domain.Entities;

public sealed class CategoryTests
{
    [Fact]
    public void Create_Should_NormalizeCodeAndSetActive_When_ValidDataProvided()
    {
        var data = new CategoryCreationData("  ELEC-01 ", "  Electronics  ", "  Consumer devices ");
        var result = Category.Create(data);
        result.IsError.Should().BeFalse();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.Code.Should().Be("elec-01");
        result.Value.Name.Should().Be("Electronics");
        result.Value.Description.Should().Be("Consumer devices");
        result.Value.IsActive.Should().BeTrue();
        result.Value.DomainEvents.Should().ContainSingle(e => e is CategoryCreatedDomainEvent);
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("code with spaces")]
    [InlineData("abcdefghijklmnopqrstuvwxyz12345")]
    public void Create_Should_ReturnValidationError_When_CodeIsInvalid(string invalidCode)
    {
        var data = new CategoryCreationData(invalidCode, "Valid Name", null);
        var result = Category.Create(data);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("Category.InvalidCode");
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    public void Create_Should_ReturnValidationError_When_NameIsTooShort(string invalidName)
    {
        var data = new CategoryCreationData("valid-code", invalidName, null);
        var result = Category.Create(data);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("Category.InvalidName");
    }

    [Fact]
    public void Create_Should_ReturnValidationError_When_DescriptionExceedsMaxLength()
    {
        var longDescription = new string('x', 501);
        var data = new CategoryCreationData("valid-code", "Valid Name", longDescription);
        var result = Category.Create(data);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("Category.InvalidDescription");
    }

    [Fact]
    public void Update_Should_UpdateFieldsAndRaiseDomainEvent_When_ValidDataProvided()
    {
        var category = Category.Create(new CategoryCreationData("old-code", "Old Name", "Old desc")).Value;
        category.ClearDomainEvents();
        var result = category.Update(new CategoryUpdateData(" NEW-CODE ", " New Name ", " Updated ", false));
        result.IsError.Should().BeFalse();
        category.Code.Should().Be("new-code");
        category.Name.Should().Be("New Name");
        category.Description.Should().Be("Updated");
        category.IsActive.Should().BeFalse();
        category.UpdatedAtUtc.Should().NotBeNull();
        category.DomainEvents.Should().ContainSingle(e => e is CategoryUpdatedDomainEvent);
    }

    [Fact]
    public void MarkDeleted_Should_RaiseCategoryDeletedDomainEvent()
    {
        var category = Category.Create(new CategoryCreationData("del-code", "To Delete", null)).Value;
        category.ClearDomainEvents();
        category.MarkDeleted();
        category.DomainEvents.Should().ContainSingle(e => e is CategoryDeletedDomainEvent);
    }
}
