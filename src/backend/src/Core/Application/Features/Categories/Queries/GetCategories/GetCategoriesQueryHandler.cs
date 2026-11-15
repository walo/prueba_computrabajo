using Application.Common.Interfaces;
using Application.Features.Categories.DTOs;
using ErrorOr;
using MediatR;

namespace Application.Features.Categories.Queries.GetCategories;

public sealed class GetCategoriesQueryHandler(ICategoryFinder categoryFinder)
    : IRequestHandler<GetCategoriesQuery, ErrorOr<PagedCategoriesResponse>>
{
    public async Task<ErrorOr<PagedCategoriesResponse>> Handle(GetCategoriesQuery query, CancellationToken ct)
    {
        return await categoryFinder.GetPagedAsync(query.Filter, ct);
    }
}
