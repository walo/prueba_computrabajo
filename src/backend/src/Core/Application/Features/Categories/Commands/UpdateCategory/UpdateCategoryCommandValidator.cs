using FluentValidation;

namespace Application.Features.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request is not null, ConfigureRequestRules);
    }

    private void ConfigureRequestRules()
    {
        RuleFor(x => x.Request.Code)
            .NotEmpty()
            .Length(2, 30)
            .Matches("^[a-zA-Z0-9_-]+$")
            .WithMessage("El código solo admite letras, números, guiones y guion bajo.");
        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .Length(2, 100);
        RuleFor(x => x.Request.Description)
            .MaximumLength(500);
    }
}
