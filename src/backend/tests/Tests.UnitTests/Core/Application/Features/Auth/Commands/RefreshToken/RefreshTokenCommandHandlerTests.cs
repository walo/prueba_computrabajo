using Application.Common.Interfaces;
using Application.Features.Auth.Commands.RefreshToken;
using Application.Features.Auth.DTOs;
using Domain.Entities;
using Domain.Primitives;
using ErrorOr;
using FluentAssertions;
using Moq;
using RefreshTokenEntity = Domain.Entities.RefreshToken;

namespace Tests.UnitTests.Core.Application.Features.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly Mock<IAuthFinder> _authFinderMock = new();
    private readonly Mock<IJwtProvider> _jwtProviderMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _handler = new RefreshTokenCommandHandler(
            _refreshTokenRepositoryMock.Object,
            _authFinderMock.Object,
            _jwtProviderMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_RevokeAllSessions_When_TokenReuseDetected()
    {
        var userId = Guid.NewGuid();
        var reusedToken = RefreshTokenEntity.Create(new RefreshTokenCreationData(userId, "used-hash", 7));
        reusedToken.MarkAsUsed("next-hash");
        var otherActiveToken = RefreshTokenEntity.Create(new RefreshTokenCreationData(userId, "active-hash", 7));
        _jwtProviderMock.Setup(j => j.HashToken("reused-raw")).Returns("used-hash");
        _refreshTokenRepositoryMock.Setup(r => r.GetByHashAsync("used-hash", It.IsAny<CancellationToken>())).ReturnsAsync(reusedToken);
        _refreshTokenRepositoryMock.Setup(r => r.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync([otherActiveToken]);
        var result = await _handler.Handle(new RefreshTokenCommand("reused-raw"), CancellationToken.None);
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Auth.RefreshTokenReused");
        otherActiveToken.IsActive.Should().BeFalse();
        _refreshTokenRepositoryMock.Verify(r => r.UpdateAsync(otherActiveToken, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_RotateTokens_When_RefreshTokenIsActive()
    {
        var userId = Guid.NewGuid();
        var activeToken = RefreshTokenEntity.Create(new RefreshTokenCreationData(userId, "valid-hash", 7));
        var expiresAt = DateTime.UtcNow.AddMinutes(15);
        _jwtProviderMock.Setup(j => j.HashToken("valid-raw")).Returns("valid-hash");
        _refreshTokenRepositoryMock.Setup(r => r.GetByHashAsync("valid-hash", It.IsAny<CancellationToken>())).ReturnsAsync(activeToken);
        _authFinderMock.Setup(f => f.GetUserIdentityByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(new UserIdentityDto(userId, "admin@test.com", "Admin", "ADMIN", true));
        _jwtProviderMock.Setup(j => j.GenerateAccessToken(It.IsAny<JwtTokenRequest>())).Returns(new GeneratedAccessToken("new-jwt", "jti-2", expiresAt));
        _jwtProviderMock.Setup(j => j.GenerateRefreshToken()).Returns(new GeneratedRefreshToken("new-raw", "new-hash"));
        var result = await _handler.Handle(new RefreshTokenCommand("valid-raw"), CancellationToken.None);
        result.IsError.Should().BeFalse();
        result.Value.AccessToken.Should().Be("new-jwt");
        result.Value.RefreshToken.Should().Be("new-raw");
        activeToken.IsUsed.Should().BeTrue();
        _refreshTokenRepositoryMock.Verify(r => r.AddAsync(It.IsAny<RefreshTokenEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
