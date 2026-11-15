using Application.Common.Interfaces;
using Application.Features.Categories.DTOs;
using Application.Features.Categories.Queries.GetCategories;
using Application.Features.Categories.Queries.GetCategoryById;
using Domain.Entities;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace Tests.UnitTests.Core.Application.Features.Categories;

public sealed class GetCategoriesQueryHandlerTests
{
    private readonly Mock<ICategoryFinder> _finderMock = new();

    [Fact]
    public async Task GetCategoriesHandle_Should_ReturnPagedCategories_When_FilterIsValid()
    {
        var handler = new GetCategoriesQueryHandler(_finderMock.Object);
        var filter = new CategoryFilter(1, 10, "elec", true);
        var expected = new PagedCategoriesResponse([], 1, 10, 0);
        _finderMock.Setup(f => f.GetPagedAsync(filter, It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var result = await handler.Handle(new GetCategoriesQuery(filter), CancellationToken.None);
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetCategoryByIdHandle_Should_ReturnNotFound_When_CategoryDoesNotExist()
    {
        var handler = new GetCategoryByIdQueryHandler(_finderMock.Object);
        var id = Guid.NewGuid();
        _finderMock.Setup(f => f.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((CategoryResponse?)null);
        var result = await handler.Handle(new GetCategoryByIdQuery(id), CancellationToken.None);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Code.Should().Be("Category.NotFound");
    }

    [Fact]
    public async Task GetCategoryByIdHandle_Should_ReturnCategory_When_Found()
    {
        var handler = new GetCategoryByIdQueryHandler(_finderMock.Object);
        var id = Guid.NewGuid();
        var expected = new CategoryResponse(id, "elec-01", "Electronics", null, true, DateTime.UtcNow, null);
        _finderMock.Setup(f => f.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var result = await handler.Handle(new GetCategoryByIdQuery(id), CancellationToken.None);
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(expected);
    }
}
