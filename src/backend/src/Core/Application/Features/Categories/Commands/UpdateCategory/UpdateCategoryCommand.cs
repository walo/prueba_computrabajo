using Application.Features.Categories.DTOs;
using ErrorOr;
using MediatR;

namespace Application.Features.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(Guid Id, UpdateCategoryRequest Request) : IRequest<ErrorOr<CategoryResponse>>;
