using Application.Features.Products.Queries.GetProductsByCategory;
using Domain.ValueObjects;
using FluentAssertions;

namespace Tests.UnitTests.Core.Application.Features.Products;

public sealed class GetProductsByCategoryQueryValidatorTests
{
    private readonly GetProductsByCategoryQueryValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_CategoryIdAndFilterAreValid()
    {
        var query = new GetProductsByCategoryQuery(
            new CategoryProductsQuery(Guid.NewGuid(), new CategoryProductsFilter(1, 20, "lap", true, false)));
        var result = _validator.Validate(query);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_RequestOrFilterIsNull()
    {
        var nullRequestResult = _validator.Validate(new GetProductsByCategoryQuery(null!));
        var nullFilterResult = _validator.Validate(
            new GetProductsByCategoryQuery(new CategoryProductsQuery(Guid.NewGuid(), null!)));
        nullRequestResult.IsValid.Should().BeFalse();
        nullFilterResult.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Fail_When_CategoryIdIsEmpty()
    {
        var query = new GetProductsByCategoryQuery(
            new CategoryProductsQuery(Guid.Empty, new CategoryProductsFilter()));
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(0, 10, null)]
    [InlineData(1, 0, null)]
    [InlineData(1, 101, null)]
    [InlineData(1, 10, "overflow")]
    public void Validate_Should_Fail_When_PaginationOrSearchIsInvalid(int page, int pageSize, string? searchMode)
    {
        var search = searchMode == "overflow" ? new string('a', 151) : searchMode;
        var query = new GetProductsByCategoryQuery(
            new CategoryProductsQuery(Guid.NewGuid(), new CategoryProductsFilter(page, pageSize, search)));
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
    }
}
