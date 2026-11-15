using Application.Features.Categories.Commands.CreateCategory;
using Application.Features.Categories.Commands.DeleteCategory;
using Application.Features.Categories.Commands.UpdateCategory;
using Application.Features.Categories.DTOs;
using Application.Features.Categories.Queries.GetCategories;
using Application.Features.Categories.Queries.GetCategoryById;
using Domain.Entities;
using FluentAssertions;

namespace Tests.UnitTests.Core.Application.Features.Categories;

public sealed class CategoryValidatorsTests
{
    [Fact]
    public void CreateValidator_Should_Fail_When_CodeContainsSpaces()
    {
        var validator = new CreateCategoryCommandValidator();
        var command = new CreateCategoryCommand(new CreateCategoryRequest("bad code", "Electronics", null));
        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void UpdateValidator_Should_Fail_When_IdIsEmpty()
    {
        var validator = new UpdateCategoryCommandValidator();
        var command = new UpdateCategoryCommand(Guid.Empty, new UpdateCategoryRequest("elec-01", "Electronics", null, true));
        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void DeleteValidator_Should_Fail_When_IdIsEmpty()
    {
        var validator = new DeleteCategoryCommandValidator();
        var result = validator.Validate(new DeleteCategoryCommand(Guid.Empty));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void GetCategoriesValidator_Should_Fail_When_PageSizeExceedsMaximum()
    {
        var validator = new GetCategoriesQueryValidator();
        var query = new GetCategoriesQuery(new CategoryFilter(1, 200));
        var result = validator.Validate(query);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void GetCategoriesValidator_Should_Pass_When_FilterIsValid()
    {
        var validator = new GetCategoriesQueryValidator();
        var query = new GetCategoriesQuery(new CategoryFilter(1, 10, "tech", true));
        var result = validator.Validate(query);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GetCategoryByIdValidator_Should_Fail_When_IdIsEmpty()
    {
        var validator = new GetCategoryByIdQueryValidator();
        var result = validator.Validate(new GetCategoryByIdQuery(Guid.Empty));
        result.IsValid.Should().BeFalse();
    }
}
