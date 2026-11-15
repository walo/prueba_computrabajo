using Application.Common.Interfaces;
using Application.Features.Auth.Commands.Logout;
using Domain.Entities;
using Domain.Primitives;
using FluentAssertions;
using Moq;
using RefreshTokenEntity = Domain.Entities.RefreshToken;

namespace Tests.UnitTests.Core.Application.Features.Auth.Commands.Logout;

public sealed class LogoutCommandHandlerTests
{
    private readonly Mock<ITokenBlacklistService> _tokenBlacklistMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly LogoutCommandHandler _handler;

    public LogoutCommandHandlerTests()
    {
        _handler = new LogoutCommandHandler(
            _tokenBlacklistMock.Object,
            _refreshTokenRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_BlacklistJtiAndRevokeTokens_When_LoggedOut()
    {
        var userId = Guid.NewGuid();
        var activeToken = RefreshTokenEntity.Create(new RefreshTokenCreationData(userId, "hash-1", 7));
        var command = new LogoutCommand(userId, "jti-abc", DateTime.UtcNow.AddMinutes(10));
        _refreshTokenRepositoryMock.Setup(r => r.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync([activeToken]);
        var result = await _handler.Handle(command, CancellationToken.None);
        result.IsError.Should().BeFalse();
        activeToken.IsActive.Should().BeFalse();
        _tokenBlacklistMock.Verify(b => b.BlacklistTokenAsync(It.Is<BlacklistTokenRequest>(req => req.Jti == "jti-abc"), It.IsAny<CancellationToken>()), Times.Once);
        _refreshTokenRepositoryMock.Verify(r => r.UpdateAsync(activeToken, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
