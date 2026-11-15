using ErrorOr;
using MediatR;

namespace Application.Features.Categories.Commands.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid Id) : IRequest<ErrorOr<Deleted>>;
