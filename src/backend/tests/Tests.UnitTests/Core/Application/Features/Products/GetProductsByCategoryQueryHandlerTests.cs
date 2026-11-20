using Application.Common.Interfaces;
using Application.Features.Products.DTOs;
using Application.Features.Products.Queries.GetProductsByCategory;
using Domain.ValueObjects;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace Tests.UnitTests.Core.Application.Features.Products;

public sealed class GetProductsByCategoryQueryHandlerTests
{
    private readonly Mock<ICategoryFinder> _categoryFinder = new();
    private readonly Mock<IProductFinder> _productFinder = new();

    [Fact]
    public async Task Handle_Should_ReturnCategoryNotFound_When_CategoryDoesNotExist()
    {
        var categoryId = Guid.NewGuid();
        _categoryFinder.Setup(f => f.ExistsAsync(categoryId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var handler = new GetProductsByCategoryQueryHandler(_categoryFinder.Object, _productFinder.Object);
        var query = new GetProductsByCategoryQuery(new CategoryProductsQuery(categoryId, new CategoryProductsFilter()));
        var result = await handler.Handle(query, CancellationToken.None);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Code.Should().Be("Category.NotFound");
        _productFinder.Verify(f => f.GetPagedAsync(It.IsAny<ProductFilter>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_ReturnPagedProductsWithPropagatedFilter_When_CategoryExists()
    {
        var categoryId = Guid.NewGuid();
        var filter = new CategoryProductsFilter(2, 25, "lap", true, true);
        var expected = new PagedProductsResponse([CreateProductResponse(categoryId)], 2, 25, 1);
        ProductFilter? capturedFilter = null;
        _categoryFinder.Setup(f => f.ExistsAsync(categoryId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _productFinder
            .Setup(f => f.GetPagedAsync(It.IsAny<ProductFilter>(), It.IsAny<CancellationToken>()))
            .Callback<ProductFilter, CancellationToken>((f, _) => capturedFilter = f)
            .ReturnsAsync(expected);
        var handler = new GetProductsByCategoryQueryHandler(_categoryFinder.Object, _productFinder.Object);
        var result = await handler.Handle(new GetProductsByCategoryQuery(new CategoryProductsQuery(categoryId, filter)), CancellationToken.None);
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(expected);
        capturedFilter.Should().Be(new ProductFilter(2, 25, "lap", categoryId, true, true));
    }

    [Fact]
    public async Task Handle_Should_ReturnEmptyPagedResult_When_CategoryHasNoProducts()
    {
        var categoryId = Guid.NewGuid();
        var expected = new PagedProductsResponse([], 1, 10, 0);
        _categoryFinder.Setup(f => f.ExistsAsync(categoryId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _productFinder.Setup(f => f.GetPagedAsync(It.IsAny<ProductFilter>(), It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var handler = new GetProductsByCategoryQueryHandler(_categoryFinder.Object, _productFinder.Object);
        var result = await handler.Handle(new GetProductsByCategoryQuery(new CategoryProductsQuery(categoryId, new CategoryProductsFilter())), CancellationToken.None);
        result.IsError.Should().BeFalse();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    private static ProductResponse CreateProductResponse(Guid categoryId)
    {
        return new ProductResponse(
            Guid.NewGuid(),
            "LAP-01",
            "Laptop Pro",
            null,
            categoryId,
            "ELEC",
            "Electronics",
            1200m,
            2,
            5,
            true,
            true,
            DateTime.UtcNow,
            null);
    }
}
