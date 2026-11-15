using Application.Features.Categories.DTOs;
using ErrorOr;
using MediatR;

namespace Application.Features.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand(CreateCategoryRequest Request) : IRequest<ErrorOr<CategoryResponse>>;
