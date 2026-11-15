using FluentValidation;

namespace Application.Features.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();
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
