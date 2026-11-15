using Application.Common.Interfaces;
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.DTOs;
using Domain.Entities;
using Domain.Primitives;
using ErrorOr;
using FluentAssertions;
using Moq;
using RefreshTokenEntity = Domain.Entities.RefreshToken;

namespace Tests.UnitTests.Core.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly Mock<IAuthFinder> _authFinderMock = new();
    private readonly Mock<IJwtProvider> _jwtProviderMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _handler = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,
            _authFinderMock.Object,
            _jwtProviderMock.Object,
            _passwordHasherMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnUnauthorized_When_UserNotFoundOrPasswordInvalid()
    {
        var command = new LoginCommand("missing@test.com", "WrongPass");
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var result = await _handler.Handle(command, CancellationToken.None);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_ReturnForbidden_When_UserIsInactive()
    {
        var user = User.Create(new UserCreationData("Inactive", "inactive@test.com", "hash", Guid.NewGuid()));
        user.Deactivate();
        var command = new LoginCommand("inactive@test.com", "Pass123!");
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.Verify(command.Password, user.PasswordHash)).Returns(true);
        var result = await _handler.Handle(command, CancellationToken.None);
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_ReturnTokensAndPersistRefreshToken_When_CredentialsAreValid()
    {
        var user = User.Create(new UserCreationData("Admin", "admin@test.com", "hash", Guid.NewGuid()));
        var command = new LoginCommand("admin@test.com", "Admin123*");
        var expiresAt = DateTime.UtcNow.AddMinutes(15);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.Verify(command.Password, user.PasswordHash)).Returns(true);
        _authFinderMock.Setup(f => f.GetUserIdentityByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new UserIdentityDto(user.Id, user.Email, user.FullName, "ADMIN", true));
        _jwtProviderMock.Setup(j => j.GenerateAccessToken(It.IsAny<JwtTokenRequest>())).Returns(new GeneratedAccessToken("jwt-token", "jti-1", expiresAt));
        _jwtProviderMock.Setup(j => j.GenerateRefreshToken()).Returns(new GeneratedRefreshToken("raw-refresh", "hash-refresh"));
        var result = await _handler.Handle(command, CancellationToken.None);
        result.IsError.Should().BeFalse();
        result.Value.AccessToken.Should().Be("jwt-token");
        result.Value.RefreshToken.Should().Be("raw-refresh");
        _refreshTokenRepositoryMock.Verify(r => r.AddAsync(It.IsAny<RefreshTokenEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
