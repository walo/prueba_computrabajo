using FluentValidation;

namespace Application.Features.Products.Queries.GetProductsByCategory;

public sealed class GetProductsByCategoryQueryValidator : AbstractValidator<GetProductsByCategoryQuery>
{
    public GetProductsByCategoryQueryValidator()
    {
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request is not null, ConfigureRequestRules);
    }

    private void ConfigureRequestRules()
    {
        RuleFor(x => x.Request.CategoryId).NotEmpty();
        RuleFor(x => x.Request.Filter).NotNull();
        When(x => x.Request.Filter is not null, ConfigureFilterRules);
    }

    private void ConfigureFilterRules()
    {
        RuleFor(x => x.Request.Filter.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Request.Filter.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Request.Filter.Search).MaximumLength(150);
    }
}
