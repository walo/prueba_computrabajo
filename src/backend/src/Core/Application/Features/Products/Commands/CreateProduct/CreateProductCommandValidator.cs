using FluentValidation;

namespace Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request is not null, ConfigureRequestRules);
    }

    private void ConfigureRequestRules()
    {
        RuleFor(x => x.Request.Sku)
            .NotEmpty()
            .Length(2, 30)
            .Matches("^[a-zA-Z0-9_-]+$")
            .WithMessage("El SKU solo admite letras, números, guiones y guion bajo.");
        RuleFor(x => x.Request.Name).NotEmpty().Length(2, 150);
        RuleFor(x => x.Request.Description).MaximumLength(500);
        RuleFor(x => x.Request.CategoryId).NotEmpty();
        RuleFor(x => x.Request.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.InitialStock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.MinStock).GreaterThanOrEqualTo(0);
    }
}
