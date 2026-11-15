using Application.Common.Interfaces;
using Application.Features.Categories.DTOs;
using Domain.Entities;
using Domain.Primitives;
using ErrorOr;
using MediatR;

namespace Application.Features.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateCategoryCommand, ErrorOr<CategoryResponse>>
{
    public async Task<ErrorOr<CategoryResponse>> Handle(CreateCategoryCommand command, CancellationToken ct)
    {
        var filter = new CategoryUniquenessFilter(
            command.Request.Code.Trim().ToLowerInvariant(),
            command.Request.Name.Trim());
        var conflictError = await ValidateUniquenessAsync(filter, ct);
        if (conflictError is not null)
        {
            return conflictError.Value;
        }
        var creationData = new CategoryCreationData(filter.Code, filter.Name, command.Request.Description);
        var categoryResult = Category.Create(creationData);
        if (categoryResult.IsError)
        {
            return categoryResult.Errors;
        }
        await categoryRepository.AddAsync(categoryResult.Value, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return MapToResponse(categoryResult.Value);
    }

    private async Task<Error?> ValidateUniquenessAsync(CategoryUniquenessFilter filter, CancellationToken ct)
    {
        var existingByCode = await categoryRepository.GetByCodeAsync(filter.Code, ct);
        if (existingByCode is not null)
        {
            return Error.Conflict("Category.DuplicateCode", $"Ya existe una categoría con el código '{filter.Code}'.");
        }
        var existingByName = await categoryRepository.GetByNameAsync(filter.Name, ct);
        if (existingByName is not null)
        {
            return Error.Conflict("Category.DuplicateName", $"Ya existe una categoría con el nombre '{filter.Name}'.");
        }

        return null;
    }

    private static CategoryResponse MapToResponse(Category category)
    {
        return new CategoryResponse(
            category.Id,
            category.Code,
            category.Name,
            category.Description,
            category.IsActive,
            category.CreatedAtUtc,
            category.UpdatedAtUtc);
    }
}
