using Application.Common.Interfaces;
using Application.Features.Products.DTOs;
using Application.Features.Products.Queries.GetProductById;
using Application.Features.Products.Queries.GetProducts;
using Domain.ValueObjects;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace Tests.UnitTests.Core.Application.Features.Products;

public sealed class GetProductsQueryHandlerTests
{
    private readonly Mock<IProductFinder> _productFinder = new();

    [Fact]
    public async Task GetProducts_Should_ReturnPagedResultFromFinder()
    {
        var filter = new ProductFilter(1, 10, "lap", null, true, true);
        var expected = new PagedProductsResponse(
            [new ProductResponse(Guid.NewGuid(), "LAP-01", "Laptop", null, Guid.NewGuid(), "elec", "Electronics", 900m, 2, 5, true, true, DateTime.UtcNow, null)],
            1,
            10,
            1);
        _productFinder.Setup(f => f.GetPagedAsync(filter, It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var handler = new GetProductsQueryHandler(_productFinder.Object);
        var result = await handler.Handle(new GetProductsQuery(filter), CancellationToken.None);
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetProductById_Should_ReturnNotFound_When_ProductDoesNotExist()
    {
        var id = Guid.NewGuid();
        _productFinder.Setup(f => f.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((ProductResponse?)null);
        var handler = new GetProductByIdQueryHandler(_productFinder.Object);
        var result = await handler.Handle(new GetProductByIdQuery(id), CancellationToken.None);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Code.Should().Be("Product.NotFound");
    }
}
