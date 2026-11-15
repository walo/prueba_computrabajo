using Application.Common.Interfaces;
using Application.Features.Categories.DTOs;
using Domain.Entities;
using Domain.Primitives;
using ErrorOr;
using MediatR;

namespace Application.Features.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateCategoryCommand, ErrorOr<CategoryResponse>>
{
    public async Task<ErrorOr<CategoryResponse>> Handle(UpdateCategoryCommand command, CancellationToken ct)
    {
        var category = await categoryRepository.GetByIdAsync(command.Id, ct);
        if (category is null)
        {
            return Error.NotFound("Category.NotFound", "La categoría solicitada no existe.");
        }
        var filter = new CategoryUniquenessFilter(
            command.Request.Code.Trim().ToLowerInvariant(),
            command.Request.Name.Trim(),
            command.Id);
        var conflictError = await ValidateUniquenessAsync(filter, ct);
        if (conflictError is not null)
        {
            return conflictError.Value;
        }

        return await ApplyAndSaveAsync(category, command.Request, ct);
    }

    private async Task<Error?> ValidateUniquenessAsync(CategoryUniquenessFilter filter, CancellationToken ct)
    {
        var existingByCode = await categoryRepository.GetByCodeAsync(filter.Code, ct);
        if (existingByCode is not null && existingByCode.Id != filter.ExcludedId)
        {
            return Error.Conflict("Category.DuplicateCode", $"Ya existe otra categoría con el código '{filter.Code}'.");
        }
        var existingByName = await categoryRepository.GetByNameAsync(filter.Name, ct);
        if (existingByName is not null && existingByName.Id != filter.ExcludedId)
        {
            return Error.Conflict("Category.DuplicateName", $"Ya existe otra categoría con el nombre '{filter.Name}'.");
        }

        return null;
    }

    private async Task<ErrorOr<CategoryResponse>> ApplyAndSaveAsync(
        Category category,
        UpdateCategoryRequest request,
        CancellationToken ct)
    {
        var updateData = new CategoryUpdateData(request.Code, request.Name, request.Description, request.IsActive);
        var updateResult = category.Update(updateData);
        if (updateResult.IsError)
        {
            return updateResult.Errors;
        }
        await categoryRepository.UpdateAsync(category, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return MapToResponse(category);
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
