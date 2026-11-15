using System.Text.RegularExpressions;
using Domain.Events;
using Domain.Primitives;
using Domain.ValueObjects;
using ErrorOr;

namespace Domain.Entities;

public sealed partial class Product : AggregateRoot<Guid>
{
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid CategoryId { get; private set; }
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public int MinStock { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; private set; }

    private Product()
    {
    }

    public static ErrorOr<Product> Create(ProductCreationData data)
    {
        var normalized = NormalizeCreationData(data);
        var validationError = ValidateCreationData(normalized);
        if (validationError is not null)
        {
            return validationError.Value;
        }
        var product = BuildProduct(normalized);
        product.AddDomainEvent(new ProductCreatedDomainEvent(product.Id, product.Sku, product.Name, product.CategoryId));

        return product;
    }

    public ErrorOr<Updated> Update(ProductUpdateData data)
    {
        var normalized = NormalizeUpdateData(data);
        var validationError = ValidateUpdateData(normalized);
        if (validationError is not null)
        {
            return validationError.Value;
        }
        ApplyUpdateData(normalized);
        AddDomainEvent(new ProductUpdatedDomainEvent(Id, Sku, Name, CategoryId, IsActive));

        return Result.Updated;
    }

    public ErrorOr<Updated> AdjustStock(int newStock)
    {
        if (newStock < 0)
        {
            return Error.Validation("Product.InsufficientStock", "El stock resultante no puede ser negativo.");
        }
        Stock = newStock;
        UpdatedAtUtc = DateTime.UtcNow;

        return Result.Updated;
    }

    public bool IsLowStock()
    {
        return Stock <= MinStock;
    }

    public void MarkDeleted()
    {
        AddDomainEvent(new ProductDeletedDomainEvent(Id, Sku));
    }

    private static ProductCreationData NormalizeCreationData(ProductCreationData data)
    {
        var sku = (data.Sku ?? string.Empty).Trim().ToUpperInvariant();
        var name = (data.Name ?? string.Empty).Trim();
        var description = string.IsNullOrWhiteSpace(data.Description) ? null : data.Description.Trim();

        return new ProductCreationData(sku, name, description, data.CategoryId, data.Price, data.InitialStock, data.MinStock);
    }

    private static ProductUpdateData NormalizeUpdateData(ProductUpdateData data)
    {
        var sku = (data.Sku ?? string.Empty).Trim().ToUpperInvariant();
        var name = (data.Name ?? string.Empty).Trim();
        var description = string.IsNullOrWhiteSpace(data.Description) ? null : data.Description.Trim();

        return new ProductUpdateData(sku, name, description, data.CategoryId, data.Price, data.MinStock, data.IsActive);
    }

    private static Error? ValidateCreationData(ProductCreationData data)
    {
        var commonError = ValidateUpdateData(
            new ProductUpdateData(data.Sku, data.Name, data.Description, data.CategoryId, data.Price, data.MinStock, true));
        if (commonError is not null)
        {
            return commonError;
        }
        if (data.InitialStock < 0)
        {
            return Error.Validation("Product.InvalidInitialStock", "El stock inicial no puede ser negativo.");
        }

        return null;
    }

    private static Error? ValidateUpdateData(ProductUpdateData data)
    {
        return ValidateTextFields(data) ?? ValidateNumericAndCategoryFields(data);
    }

    private static Error? ValidateTextFields(ProductUpdateData data)
    {
        if (!SkuPattern().IsMatch(data.Sku))
        {
            return Error.Validation("Product.InvalidSku", "El SKU debe tener entre 2 y 30 caracteres alfanuméricos o guiones.");
        }
        if (data.Name.Length is < 2 or > 150)
        {
            return Error.Validation("Product.InvalidName", "El nombre del producto debe tener entre 2 y 150 caracteres.");
        }
        if (data.Description is { Length: > 500 })
        {
            return Error.Validation("Product.InvalidDescription", "La descripción del producto no puede superar los 500 caracteres.");
        }

        return null;
    }

    private static Error? ValidateNumericAndCategoryFields(ProductUpdateData data)
    {
        if (data.CategoryId == Guid.Empty)
        {
            return Error.Validation("Product.InvalidCategory", "La categoría del producto es obligatoria.");
        }
        if (data.Price < 0)
        {
            return Error.Validation("Product.InvalidPrice", "El precio del producto no puede ser negativo.");
        }
        if (data.MinStock < 0)
        {
            return Error.Validation("Product.InvalidMinStock", "El stock mínimo no puede ser negativo.");
        }

        return null;
    }

    private static Product BuildProduct(ProductCreationData data)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Sku = data.Sku,
            Name = data.Name,
            Description = data.Description,
            CategoryId = data.CategoryId,
            Price = data.Price,
            Stock = data.InitialStock,
            MinStock = data.MinStock,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    private void ApplyUpdateData(ProductUpdateData data)
    {
        Sku = data.Sku;
        Name = data.Name;
        Description = data.Description;
        CategoryId = data.CategoryId;
        Price = data.Price;
        MinStock = data.MinStock;
        IsActive = data.IsActive;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    [GeneratedRegex("^[A-Z0-9_-]{2,30}$")]
    private static partial Regex SkuPattern();
}
