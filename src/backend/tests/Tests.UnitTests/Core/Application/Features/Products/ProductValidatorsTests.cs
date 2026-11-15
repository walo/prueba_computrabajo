using Application.Features.Products.Commands.CreateProduct;
using Application.Features.Products.Commands.DeleteProduct;
using Application.Features.Products.Commands.UpdateProduct;
using Application.Features.Products.DTOs;
using Application.Features.Products.Queries.GetProductById;
using Application.Features.Products.Queries.GetProducts;
using Domain.ValueObjects;
using FluentAssertions;

namespace Tests.UnitTests.Core.Application.Features.Products;

public sealed class ProductValidatorsTests
{
    [Fact]
    public void CreateValidator_Should_Fail_When_PayloadHasInvalidValues()
    {
        var validator = new CreateProductCommandValidator();
        var command = new CreateProductCommand(
            new CreateProductRequest("A", "", null, Guid.Empty, -5m, -1, -2));
        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(5);
    }

    [Fact]
    public void UpdateValidator_Should_Pass_When_PayloadIsValid()
    {
        var validator = new UpdateProductCommandValidator();
        var command = new UpdateProductCommand(
            Guid.NewGuid(),
            new UpdateProductRequest("SKU-01", "Laptop", "Pro model", Guid.NewGuid(), 100m, 2, true));
        var result = validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void QueryValidators_Should_ValidatePaginationAndIds()
    {
        var listValidator = new GetProductsQueryValidator();
        var byIdValidator = new GetProductByIdQueryValidator();
        var deleteValidator = new DeleteProductCommandValidator();
        listValidator.Validate(new GetProductsQuery(new ProductFilter(0, 200))).IsValid.Should().BeFalse();
        listValidator.Validate(new GetProductsQuery(new ProductFilter(1, 20))).IsValid.Should().BeTrue();
        byIdValidator.Validate(new GetProductByIdQuery(Guid.Empty)).IsValid.Should().BeFalse();
        deleteValidator.Validate(new DeleteProductCommand(Guid.Empty)).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validators_Should_FailGracefully_When_PayloadOrFilterIsNull()
    {
        var createResult = new CreateProductCommandValidator().Validate(new CreateProductCommand(null!));
        var updateResult = new UpdateProductCommandValidator().Validate(new UpdateProductCommand(Guid.NewGuid(), null!));
        var queryResult = new GetProductsQueryValidator().Validate(new GetProductsQuery(null!));
        createResult.IsValid.Should().BeFalse();
        updateResult.IsValid.Should().BeFalse();
        queryResult.IsValid.Should().BeFalse();
    }
}
