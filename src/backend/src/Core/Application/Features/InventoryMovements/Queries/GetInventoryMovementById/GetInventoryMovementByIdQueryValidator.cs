using FluentValidation;

namespace Application.Features.InventoryMovements.Queries.GetInventoryMovementById;

public sealed class GetInventoryMovementByIdQueryValidator : AbstractValidator<GetInventoryMovementByIdQuery>
{
    public GetInventoryMovementByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
