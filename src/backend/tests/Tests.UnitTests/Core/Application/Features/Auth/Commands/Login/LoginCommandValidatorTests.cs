using Application.Features.Auth.Commands.Login;
using FluentValidation.TestHelper;

namespace Tests.UnitTests.Core.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    public void Should_HaveError_When_EmailIsInvalid(string invalidEmail)
    {
        var command = new LoginCommand(invalidEmail, "Admin123*");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_HaveError_When_PasswordIsEmpty()
    {
        var command = new LoginCommand("admin@enterprise.local", string.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_NotHaveError_When_CommandIsValid()
    {
        var command = new LoginCommand("admin@enterprise.local", "Admin123*");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
