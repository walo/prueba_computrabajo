using Application.Features.Categories.DTOs;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ICategoryFinder
{
    Task<PagedCategoriesResponse> GetPagedAsync(CategoryFilter filter, CancellationToken ct = default);
    Task<CategoryResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);
}
