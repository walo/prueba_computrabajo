using Application.Common.Interfaces;
using Application.Features.InventoryMovements.DTOs;
using Domain.Entities;
using Domain.Primitives;
using Domain.ValueObjects;
using ErrorOr;
using MediatR;

namespace Application.Features.InventoryMovements.Commands.CreateInventoryMovement;

public sealed class CreateInventoryMovementCommandHandler(
    IInventoryMovementRepository movementRepository,
    IProductRepository productRepository,
    IUserRepository userRepository,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateInventoryMovementCommand, ErrorOr<InventoryMovementResponse>>
{
    public async Task<ErrorOr<InventoryMovementResponse>> Handle(
        CreateInventoryMovementCommand command,
        CancellationToken ct)
    {
        if (currentUserContext.UserId is null || currentUserContext.UserId.Value == Guid.Empty)
        {
            return Error.Unauthorized("InventoryMovement.UnauthorizedUser", "El usuario autenticado es obligatorio.");
        }
        var productResult = await ResolveActiveProductAsync(command.Request.ProductId, ct);
        if (productResult.IsError)
        {
            return productResult.Errors;
        }

        return await PersistMovementAsync(command.Request, productResult.Value, ct);
    }

    private async Task<ErrorOr<Product>> ResolveActiveProductAsync(Guid productId, CancellationToken ct)
    {
        var product = await productRepository.GetByIdAsync(productId, ct);
        if (product is null)
        {
            return Error.NotFound("InventoryMovement.ProductNotFound", "El producto seleccionado no existe.");
        }
        if (!product.IsActive)
        {
            return Error.Conflict("InventoryMovement.ProductInactive", "No se pueden registrar movimientos sobre un producto inactivo.");
        }

        return product;
    }

    private async Task<ErrorOr<InventoryMovementResponse>> PersistMovementAsync(
        CreateInventoryMovementRequest request,
        Product product,
        CancellationToken ct)
    {
        var userId = currentUserContext.UserId!.Value;
        var data = new InventoryMovementCreationData(
            product.Id, request.Type, request.Quantity, product.Stock, request.Reason, userId);
        var movementResult = InventoryMovement.Create(data);
        if (movementResult.IsError)
        {
            return movementResult.Errors;
        }
        var persisted = await movementRepository.AddAsync(movementResult.Value, ct);
        if (!persisted)
        {
            return Error.Conflict("InventoryMovement.InsufficientStock", "El stock disponible es insuficiente o cambió concurrentemente.");
        }
        product.AdjustStock(movementResult.Value.NewStock);
        await unitOfWork.SaveChangesAsync(ct);

        return await BuildMovementResponseAsync(movementResult.Value, product, ct);
    }

    private async Task<string> ResolveUserEmailAsync(Guid userId, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(currentUserContext.Email))
        {
            return currentUserContext.Email.Trim();
        }
        var user = await userRepository.GetByIdAsync(userId, ct);

        return user?.Email ?? string.Empty;
    }

    private async Task<InventoryMovementResponse> BuildMovementResponseAsync(
        InventoryMovement movement,
        Product product,
        CancellationToken ct)
    {
        var userEmail = await ResolveUserEmailAsync(movement.PerformedByUserId, ct);

        return new InventoryMovementResponse(
            movement.Id,
            movement.ProductId,
            product.Sku,
            product.Name,
            movement.Type,
            movement.Quantity,
            movement.PreviousStock,
            movement.NewStock,
            movement.Reason,
            movement.PerformedByUserId,
            userEmail,
            movement.CreatedAtUtc);
    }
}
