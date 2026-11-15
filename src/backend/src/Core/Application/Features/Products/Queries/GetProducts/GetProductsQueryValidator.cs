using FluentValidation;

namespace Application.Features.Products.Queries.GetProducts;

public sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(x => x.Filter).NotNull();
        When(x => x.Filter is not null, ConfigureFilterRules);
    }

    private void ConfigureFilterRules()
    {
        RuleFor(x => x.Filter.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Filter.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Filter.Search).MaximumLength(150);
    }
}
