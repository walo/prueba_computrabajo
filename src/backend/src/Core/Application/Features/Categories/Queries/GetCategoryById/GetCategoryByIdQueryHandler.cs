using Application.Common.Interfaces;
using Application.Features.Categories.DTOs;
using ErrorOr;
using MediatR;

namespace Application.Features.Categories.Queries.GetCategoryById;

public sealed class GetCategoryByIdQueryHandler(ICategoryFinder categoryFinder)
    : IRequestHandler<GetCategoryByIdQuery, ErrorOr<CategoryResponse>>
{
    public async Task<ErrorOr<CategoryResponse>> Handle(GetCategoryByIdQuery query, CancellationToken ct)
    {
        var category = await categoryFinder.GetByIdAsync(query.Id, ct);
        if (category is null)
        {
            return Error.NotFound("Category.NotFound", "La categoría solicitada no existe.");
        }

        return category;
    }
}
