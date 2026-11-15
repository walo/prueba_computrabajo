using Application.Common.Interfaces;
using Application.Features.Categories.Commands.CreateCategory;
using Application.Features.Categories.DTOs;
using Domain.Entities;
using Domain.Primitives;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace Tests.UnitTests.Core.Application.Features.Categories;

public sealed class CreateCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly CreateCategoryCommandHandler _handler;

    public CreateCategoryCommandHandlerTests()
    {
        _handler = new CreateCategoryCommandHandler(_repositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_CreateCategory_When_CodeAndNameAreUnique()
    {
        var request = new CreateCategoryRequest("BEV-01", "Beverages", "Cold drinks");
        _repositoryMock.Setup(r => r.GetByCodeAsync("bev-01", It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);
        _repositoryMock.Setup(r => r.GetByNameAsync("Beverages", It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);
        var result = await _handler.Handle(new CreateCategoryCommand(request), CancellationToken.None);
        result.IsError.Should().BeFalse();
        result.Value.Code.Should().Be("bev-01");
        result.Value.Name.Should().Be("Beverages");
        result.Value.IsActive.Should().BeTrue();
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnConflict_When_CodeAlreadyExists()
    {
        var existing = Category.Create(new CategoryCreationData("bev-01", "Existing", null)).Value;
        var request = new CreateCategoryRequest("BEV-01", "New Name", null);
        _repositoryMock.Setup(r => r.GetByCodeAsync("bev-01", It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        var result = await _handler.Handle(new CreateCategoryCommand(request), CancellationToken.None);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
        result.FirstError.Code.Should().Be("Category.DuplicateCode");
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_ReturnConflict_When_NameAlreadyExists()
    {
        var existing = Category.Create(new CategoryCreationData("other-01", "Beverages", null)).Value;
        var request = new CreateCategoryRequest("bev-02", "Beverages", null);
        _repositoryMock.Setup(r => r.GetByCodeAsync("bev-02", It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);
        _repositoryMock.Setup(r => r.GetByNameAsync("Beverages", It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        var result = await _handler.Handle(new CreateCategoryCommand(request), CancellationToken.None);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
        result.FirstError.Code.Should().Be("Category.DuplicateName");
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
