using Application.Features.Items.Commands.CreateItem;
using FluentValidation.TestHelper;
using Xunit;

namespace Tests.Application.Features.Items.Validators;

public sealed class CreateItemCommandValidatorTests
{
    private readonly CreateItemCommandValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Should_HaveError_When_CodeIsEmpty(string? invalidCode)
    {
        var command = new CreateItemCommand(invalidCode!, "Nombre Válido", "Desc", 100m);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Should_HaveError_When_PriceIsZeroOrNegative(decimal invalidPrice)
    {
        var command = new CreateItemCommand("CODE-1", "Nombre Válido", "Desc", invalidPrice);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Should_NotHaveError_When_CommandIsValid()
    {
        var command = new CreateItemCommand("VALID-CODE", "Nombre Correcto", "Descripción", 25.5m);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
