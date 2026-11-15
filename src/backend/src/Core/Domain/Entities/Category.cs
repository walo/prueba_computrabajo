using System.Text.RegularExpressions;
using Domain.Events;
using Domain.Primitives;
using ErrorOr;

namespace Domain.Entities;

public sealed partial class Category : AggregateRoot<Guid>
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; private set; }

    private Category()
    {
    }

    public static ErrorOr<Category> Create(CategoryCreationData data)
    {
        var normalized = NormalizeData(new CategoryUpdateData(data.Code, data.Name, data.Description, true));
        var validationError = ValidateNormalizedData(normalized);
        if (validationError is not null)
        {
            return validationError.Value;
        }
        var category = BuildCategory(normalized);
        category.AddDomainEvent(new CategoryCreatedDomainEvent(category.Id, category.Code, category.Name));

        return category;
    }

    public ErrorOr<Updated> Update(CategoryUpdateData data)
    {
        var normalized = NormalizeData(data);
        var validationError = ValidateNormalizedData(normalized);
        if (validationError is not null)
        {
            return validationError.Value;
        }
        ApplyNormalizedData(normalized);
        AddDomainEvent(new CategoryUpdatedDomainEvent(Id, Code, Name, IsActive));

        return Result.Updated;
    }

    public void MarkDeleted()
    {
        AddDomainEvent(new CategoryDeletedDomainEvent(Id, Code));
    }

    private static CategoryUpdateData NormalizeData(CategoryUpdateData data)
    {
        var code = (data.Code ?? string.Empty).Trim().ToLowerInvariant();
        var name = (data.Name ?? string.Empty).Trim();
        var description = string.IsNullOrWhiteSpace(data.Description) ? null : data.Description.Trim();

        return new CategoryUpdateData(code, name, description, data.IsActive);
    }

    private static Error? ValidateNormalizedData(CategoryUpdateData data)
    {
        if (!CodePattern().IsMatch(data.Code))
        {
            return Error.Validation("Category.InvalidCode", "El código de categoría debe tener entre 2 y 30 caracteres alfanuméricos o guiones.");
        }
        if (data.Name.Length is < 2 or > 100)
        {
            return Error.Validation("Category.InvalidName", "El nombre de la categoría debe tener entre 2 y 100 caracteres.");
        }
        if (data.Description is { Length: > 500 })
        {
            return Error.Validation("Category.InvalidDescription", "La descripción de la categoría no puede superar los 500 caracteres.");
        }

        return null;
    }

    private static Category BuildCategory(CategoryUpdateData data)
    {
        return new Category
        {
            Id = Guid.NewGuid(),
            Code = data.Code,
            Name = data.Name,
            Description = data.Description,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    private void ApplyNormalizedData(CategoryUpdateData data)
    {
        Code = data.Code;
        Name = data.Name;
        Description = data.Description;
        IsActive = data.IsActive;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    [GeneratedRegex("^[a-z0-9_-]{2,30}$")]
    private static partial Regex CodePattern();
}
