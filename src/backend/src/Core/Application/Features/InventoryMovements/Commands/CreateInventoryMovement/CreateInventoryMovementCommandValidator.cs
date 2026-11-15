using Domain.Constants;
using FluentValidation;

namespace Application.Features.InventoryMovements.Commands.CreateInventoryMovement;

public sealed class CreateInventoryMovementCommandValidator : AbstractValidator<CreateInventoryMovementCommand>
{
    public CreateInventoryMovementCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request is not null, ConfigureRequestRules);
    }

    private void ConfigureRequestRules()
    {
        RuleFor(x => x.Request.ProductId).NotEmpty();
        RuleFor(x => x.Request.Type)
            .NotEmpty()
            .Must(type => InventoryMovementTypes.IsValid(InventoryMovementTypes.Normalize(type)))
            .WithMessage("El tipo de movimiento debe ser ENTRY o EXIT.");
        RuleFor(x => x.Request.Quantity).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Request.Reason)
            .NotEmpty()
            .Must(reason => !string.IsNullOrWhiteSpace(reason) && reason.Trim().Length is >= 3 and <= 300)
            .WithMessage("El motivo debe tener entre 3 y 300 caracteres.");
    }
}
