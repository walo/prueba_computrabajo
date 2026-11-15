using Application.Common.Interfaces;
using Application.Features.Categories.Commands.DeleteCategory;
using Domain.Entities;
using Domain.Primitives;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace Tests.UnitTests.Core.Application.Features.Categories;

public sealed class DeleteCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly DeleteCategoryCommandHandler _handler;

    public DeleteCategoryCommandHandlerTests()
    {
        _handler = new DeleteCategoryCommandHandler(_repositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_CategoryDoesNotExist()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);
        var result = await _handler.Handle(new DeleteCategoryCommand(id), CancellationToken.None);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Code.Should().Be("Category.NotFound");
    }

    [Fact]
    public async Task Handle_Should_ReturnConflict_When_CategoryHasAssociatedProducts()
    {
        var category = Category.Create(new CategoryCreationData("cat-01", "Drinks", null)).Value;
        _repositoryMock.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        _repositoryMock.Setup(r => r.HasAssociatedProductsAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var result = await _handler.Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
        result.FirstError.Code.Should().Be("Category.HasAssociatedProducts");
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_DeleteCategory_When_NoProductsAreAssociated()
    {
        var category = Category.Create(new CategoryCreationData("cat-01", "Drinks", null)).Value;
        _repositoryMock.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        _repositoryMock.Setup(r => r.HasAssociatedProductsAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var result = await _handler.Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None);
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
        _repositoryMock.Verify(r => r.DeleteAsync(category, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
