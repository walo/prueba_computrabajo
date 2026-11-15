using Application.Common.Interfaces;
using Application.Features.InventoryMovements.Commands.CreateInventoryMovement;
using Application.Features.InventoryMovements.DTOs;
using Domain.Constants;
using Domain.Entities;
using Domain.Primitives;
using Domain.ValueObjects;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace Tests.UnitTests.Core.Application.Features.InventoryMovements;

public sealed class CreateInventoryMovementCommandHandlerTests
{
    private readonly Mock<IInventoryMovementRepository> _movementRepository = new();
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<ICurrentUserContext> _currentUser = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task Handle_Should_CreateEntryMovementAndUpdateProductStock_When_Valid()
    {
        var userId = Guid.NewGuid();
        var product = Product.Create(new ProductCreationData("LAP-01", "Laptop", null, Guid.NewGuid(), 1000m, 10, 3)).Value;
        _currentUser.SetupGet(c => c.UserId).Returns(userId);
        _currentUser.SetupGet(c => c.Email).Returns("admin@enterprise.local");
        _productRepository.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _movementRepository.Setup(r => r.AddAsync(It.IsAny<InventoryMovement>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = BuildHandler();
        var request = new CreateInventoryMovementRequest(product.Id, InventoryMovementTypes.Entry, 5, "Supplier intake");
        var result = await handler.Handle(new CreateInventoryMovementCommand(request), CancellationToken.None);
        result.IsError.Should().BeFalse();
        result.Value.PreviousStock.Should().Be(10);
        result.Value.NewStock.Should().Be(15);
        result.Value.ProductSku.Should().Be("LAP-01");
        result.Value.PerformedByEmail.Should().Be("admin@enterprise.local");
        product.Stock.Should().Be(15);
        _movementRepository.Verify(r => r.AddAsync(It.IsAny<InventoryMovement>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnUnauthorized_When_CurrentUserIsMissing()
    {
        _currentUser.SetupGet(c => c.UserId).Returns((Guid?)null);
        var handler = BuildHandler();
        var request = new CreateInventoryMovementRequest(Guid.NewGuid(), InventoryMovementTypes.Entry, 5, "Supplier intake");
        var result = await handler.Handle(new CreateInventoryMovementCommand(request), CancellationToken.None);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
        result.FirstError.Code.Should().Be("InventoryMovement.UnauthorizedUser");
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_ProductDoesNotExist()
    {
        _currentUser.SetupGet(c => c.UserId).Returns(Guid.NewGuid());
        _productRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);
        var handler = BuildHandler();
        var request = new CreateInventoryMovementRequest(Guid.NewGuid(), InventoryMovementTypes.Entry, 5, "Supplier intake");
        var result = await handler.Handle(new CreateInventoryMovementCommand(request), CancellationToken.None);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Code.Should().Be("InventoryMovement.ProductNotFound");
    }

    [Fact]
    public async Task Handle_Should_ReturnConflict_When_ProductIsInactive()
    {
        var categoryId = Guid.NewGuid();
        var product = Product.Create(new ProductCreationData("LAP-02", "Old Laptop", null, categoryId, 500m, 5, 1)).Value;
        product.Update(new ProductUpdateData("LAP-02", "Old Laptop", null, categoryId, 500m, 1, false));
        _currentUser.SetupGet(c => c.UserId).Returns(Guid.NewGuid());
        _productRepository.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        var handler = BuildHandler();
        var request = new CreateInventoryMovementRequest(product.Id, InventoryMovementTypes.Entry, 5, "Restock");
        var result = await handler.Handle(new CreateInventoryMovementCommand(request), CancellationToken.None);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
        result.FirstError.Code.Should().Be("InventoryMovement.ProductInactive");
    }

    [Fact]
    public async Task Handle_Should_ReturnConflict_When_ExitExceedsProductStock()
    {
        var product = Product.Create(new ProductCreationData("LAP-03", "Thin Laptop", null, Guid.NewGuid(), 800m, 3, 1)).Value;
        _currentUser.SetupGet(c => c.UserId).Returns(Guid.NewGuid());
        _productRepository.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        var handler = BuildHandler();
        var request = new CreateInventoryMovementRequest(product.Id, InventoryMovementTypes.Exit, 4, "Order dispatch");
        var result = await handler.Handle(new CreateInventoryMovementCommand(request), CancellationToken.None);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
        result.FirstError.Code.Should().Be("InventoryMovement.InsufficientStock");
        _movementRepository.Verify(r => r.AddAsync(It.IsAny<InventoryMovement>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_ReturnConflict_When_ConcurrentStockUpdateFails()
    {
        var product = Product.Create(new ProductCreationData("LAP-04", "Monitor", null, Guid.NewGuid(), 300m, 5, 1)).Value;
        _currentUser.SetupGet(c => c.UserId).Returns(Guid.NewGuid());
        _productRepository.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _movementRepository.Setup(r => r.AddAsync(It.IsAny<InventoryMovement>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var handler = BuildHandler();
        var request = new CreateInventoryMovementRequest(product.Id, InventoryMovementTypes.Exit, 5, "Concurrent sale");
        var result = await handler.Handle(new CreateInventoryMovementCommand(request), CancellationToken.None);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
        result.FirstError.Code.Should().Be("InventoryMovement.InsufficientStock");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private CreateInventoryMovementCommandHandler BuildHandler()
    {
        return new CreateInventoryMovementCommandHandler(
            _movementRepository.Object,
            _productRepository.Object,
            _userRepository.Object,
            _currentUser.Object,
            _unitOfWork.Object);
    }
}
