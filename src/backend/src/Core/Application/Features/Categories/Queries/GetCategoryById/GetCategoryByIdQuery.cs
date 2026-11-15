using Application.Features.Categories.DTOs;
using ErrorOr;
using MediatR;

namespace Application.Features.Categories.Queries.GetCategoryById;

public sealed record GetCategoryByIdQuery(Guid Id) : IRequest<ErrorOr<CategoryResponse>>;
