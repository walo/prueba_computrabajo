using Application.Common.Interfaces;
using Application.Features.Categories.Commands.UpdateCategory;
using Application.Features.Categories.DTOs;
using Domain.Entities;
using Domain.Primitives;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace Tests.UnitTests.Core.Application.Features.Categories;

public sealed class UpdateCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly UpdateCategoryCommandHandler _handler;

    public UpdateCategoryCommandHandlerTests()
    {
        _handler = new UpdateCategoryCommandHandler(_repositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_CategoryDoesNotExist()
    {
        var id = Guid.NewGuid();
        var command = new UpdateCategoryCommand(id, new UpdateCategoryRequest("cat-01", "Name", null, true));
        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);
        var result = await _handler.Handle(command, CancellationToken.None);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Code.Should().Be("Category.NotFound");
    }

    [Fact]
    public async Task Handle_Should_ReturnConflict_When_CodeBelongsToAnotherCategory()
    {
        var existing = Category.Create(new CategoryCreationData("cat-01", "First", null)).Value;
        var conflicting = Category.Create(new CategoryCreationData("cat-02", "Second", null)).Value;
        var command = new UpdateCategoryCommand(existing.Id, new UpdateCategoryRequest("CAT-02", "First Updated", null, true));
        _repositoryMock.Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _repositoryMock.Setup(r => r.GetByCodeAsync("cat-02", It.IsAny<CancellationToken>())).ReturnsAsync(conflicting);
        var result = await _handler.Handle(command, CancellationToken.None);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
        result.FirstError.Code.Should().Be("Category.DuplicateCode");
    }

    [Fact]
    public async Task Handle_Should_ReturnConflict_When_NameBelongsToAnotherCategory()
    {
        var existing = Category.Create(new CategoryCreationData("cat-01", "First", null)).Value;
        var conflicting = Category.Create(new CategoryCreationData("cat-02", "Second", null)).Value;
        var command = new UpdateCategoryCommand(existing.Id, new UpdateCategoryRequest("cat-01", "Second", null, true));
        _repositoryMock.Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _repositoryMock.Setup(r => r.GetByCodeAsync("cat-01", It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _repositoryMock.Setup(r => r.GetByNameAsync("Second", It.IsAny<CancellationToken>())).ReturnsAsync(conflicting);
        var result = await _handler.Handle(command, CancellationToken.None);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
        result.FirstError.Code.Should().Be("Category.DuplicateName");
    }

    [Fact]
    public async Task Handle_Should_UpdateAndPersist_When_PayloadIsValid()
    {
        var existing = Category.Create(new CategoryCreationData("cat-01", "First", null)).Value;
        var command = new UpdateCategoryCommand(existing.Id, new UpdateCategoryRequest("cat-01", "First Updated", "Notes", false));
        _repositoryMock.Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _repositoryMock.Setup(r => r.GetByCodeAsync("cat-01", It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _repositoryMock.Setup(r => r.GetByNameAsync("First Updated", It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);
        var result = await _handler.Handle(command, CancellationToken.None);
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be("First Updated");
        result.Value.IsActive.Should().BeFalse();
        _repositoryMock.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
