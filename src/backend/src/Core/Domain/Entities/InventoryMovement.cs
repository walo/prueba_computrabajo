using Domain.Constants;
using Domain.Events;
using Domain.Primitives;
using Domain.ValueObjects;
using ErrorOr;

namespace Domain.Entities;

public sealed class InventoryMovement : AggregateRoot<Guid>
{
    public Guid ProductId { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public int PreviousStock { get; private set; }
    public int NewStock { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public Guid PerformedByUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    private InventoryMovement()
    {
    }

    public static ErrorOr<InventoryMovement> Create(InventoryMovementCreationData data)
    {
        if (data is null)
        {
            return Error.Validation("InventoryMovement.InvalidData", "Los datos del movimiento son obligatorios.");
        }
        var normalized = NormalizeData(data);
        var validationError = ValidateCreationData(normalized);
        if (validationError is not null)
        {
            return validationError.Value;
        }
        var newStock = ComputeNewStock(normalized);
        if (newStock < 0)
        {
            return Error.Conflict(
                "InventoryMovement.InsufficientStock",
                "El stock disponible es insuficiente para realizar la salida solicitada.");
        }

        return CreateWithEvent(normalized, newStock);
    }

    private static InventoryMovementCreationData NormalizeData(InventoryMovementCreationData data)
    {
        var normalizedType = InventoryMovementTypes.Normalize(data.Type);
        var normalizedReason = (data.Reason ?? string.Empty).Trim();

        return new InventoryMovementCreationData(
            data.ProductId,
            normalizedType,
            data.Quantity,
            data.PreviousStock,
            normalizedReason,
            data.PerformedByUserId);
    }

    private static Error? ValidateCreationData(InventoryMovementCreationData data)
    {
        var referenceError = ValidateReferencesAndStock(data);
        if (referenceError is not null)
        {
            return referenceError;
        }

        return ValidateTypeQuantityAndReason(data);
    }

    private static Error? ValidateReferencesAndStock(InventoryMovementCreationData data)
    {
        if (data.ProductId == Guid.Empty)
        {
            return Error.Validation("InventoryMovement.InvalidProduct", "El producto seleccionado no es válido.");
        }
        if (data.PerformedByUserId == Guid.Empty)
        {
            return Error.Unauthorized("InventoryMovement.UnauthorizedUser", "El usuario autenticado es obligatorio.");
        }
        if (data.PreviousStock < 0)
        {
            return Error.Validation("InventoryMovement.InvalidPreviousStock", "El stock anterior no puede ser negativo.");
        }

        return null;
    }

    private static Error? ValidateTypeQuantityAndReason(InventoryMovementCreationData data)
    {
        if (!InventoryMovementTypes.IsValid(data.Type))
        {
            return Error.Validation("InventoryMovement.InvalidType", "El tipo de movimiento debe ser ENTRY o EXIT.");
        }
        if (data.Quantity < 1)
        {
            return Error.Validation("InventoryMovement.InvalidQuantity", "La cantidad debe ser un entero mayor o igual a 1.");
        }
        if (data.Reason.Length is < 3 or > 300)
        {
            return Error.Validation("InventoryMovement.InvalidReason", "El motivo debe tener entre 3 y 300 caracteres.");
        }

        return null;
    }

    private static int ComputeNewStock(InventoryMovementCreationData data)
    {
        if (data.Type == InventoryMovementTypes.Entry)
        {
            return data.PreviousStock + data.Quantity;
        }

        return data.PreviousStock - data.Quantity;
    }

    private static InventoryMovement CreateWithEvent(InventoryMovementCreationData data, int newStock)
    {
        var movement = BuildMovement(data, newStock);
        movement.AddDomainEvent(new InventoryMovementCreatedDomainEvent(
            movement.Id,
            movement.ProductId,
            movement.Type,
            movement.Quantity,
            movement.PreviousStock,
            movement.NewStock,
            movement.PerformedByUserId));

        return movement;
    }

    private static InventoryMovement BuildMovement(InventoryMovementCreationData data, int newStock)
    {
        return new InventoryMovement
        {
            Id = Guid.NewGuid(),
            ProductId = data.ProductId,
            Type = data.Type,
            Quantity = data.Quantity,
            PreviousStock = data.PreviousStock,
            NewStock = newStock,
            Reason = data.Reason,
            PerformedByUserId = data.PerformedByUserId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
