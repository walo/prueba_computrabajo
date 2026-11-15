using Domain.Constants;
using FluentValidation;

namespace Application.Features.InventoryMovements.Queries.GetInventoryMovements;

public sealed class GetInventoryMovementsQueryValidator : AbstractValidator<GetInventoryMovementsQuery>
{
    public GetInventoryMovementsQueryValidator()
    {
        RuleFor(x => x.Filter).NotNull();
        When(x => x.Filter is not null, ConfigureFilterRules);
    }

    private void ConfigureFilterRules()
    {
        RuleFor(x => x.Filter.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Filter.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Filter.Search).MaximumLength(150);
        RuleFor(x => x.Filter.Type)
            .Must(type => string.IsNullOrWhiteSpace(type) || InventoryMovementTypes.IsValid(InventoryMovementTypes.Normalize(type)))
            .WithMessage("El filtro de tipo debe ser ENTRY o EXIT.");
        RuleFor(x => x.Filter)
            .Must(filter => !filter.FromDateUtc.HasValue || !filter.ToDateUtc.HasValue || filter.FromDateUtc <= filter.ToDateUtc)
            .WithMessage("La fecha inicial no puede ser mayor que la fecha final.");
    }
}
