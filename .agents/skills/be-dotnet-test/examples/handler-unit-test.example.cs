using Application.Features.Items.Commands.CreateItem;
using Domain.Entities.Features;
using Domain.Primitives;
using Domain.Repositories;
using Domain.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace Tests.Application.Features.Items.Commands;

public sealed class CreateItemCommandHandlerTests
{
    private readonly Mock<IItemRepository> _itemRepositoryMock;
    private readonly Mock<ICompanyRepository> _companyRepositoryMock;
    private readonly Mock<IItemDomainService> _itemDomainServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateItemCommandHandler _handler;

    public CreateItemCommandHandlerTests()
    {
        _itemRepositoryMock = new Mock<IItemRepository>();
        _companyRepositoryMock = new Mock<ICompanyRepository>();
        _itemDomainServiceMock = new Mock<IItemDomainService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new CreateItemCommandHandler(
            _itemRepositoryMock.Object,
            _companyRepositoryMock.Object,
            _itemDomainServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_CompanyDoesNotExist()
    {
        // Arrange
        var command = new CreateItemCommand(Guid.NewGuid(), "ITEM-001", "Item", "Descripción", 100m);
        _companyRepositoryMock
            .Setup(r => r.GetByIdAsync(command.CompanyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Company?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Company.NotFound");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_ReturnConflict_When_ItemCodeAlreadyExists()
    {
        // Arrange
        var company = Company.Create(Guid.NewGuid(), "Empresa Test", "123456789-0", "empresa@test.com");
        var command = new CreateItemCommand(company.Id, "ITEM-001", "Item", "Descripción", 100m);

        _companyRepositoryMock
            .Setup(r => r.GetByIdAsync(command.CompanyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);

        _itemDomainServiceMock
            .Setup(s => s.IsCodeUniqueAsync(command.Code, command.CompanyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Item.DuplicateCode");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_CreateItemAndSave_When_CommandIsValid()
    {
        // Arrange
        var company = Company.Create(Guid.NewGuid(), "Empresa Test", "123456789-0", "empresa@test.com");
        var command = new CreateItemCommand(company.Id, "NEW-001", "Nuevo Item", "Descripción", 50m);

        _companyRepositoryMock
            .Setup(r => r.GetByIdAsync(command.CompanyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);

        _itemDomainServiceMock
            .Setup(s => s.IsCodeUniqueAsync(command.Code, command.CompanyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeEmpty();

        _itemRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

