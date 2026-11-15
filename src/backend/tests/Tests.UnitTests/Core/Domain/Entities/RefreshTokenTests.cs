using Domain.Entities;
using FluentAssertions;

namespace Tests.UnitTests.Core.Domain.Entities;

public sealed class RefreshTokenTests
{
    [Fact]
    public void Create_Should_CreateActiveToken_When_PositiveExpirationDaysProvided()
    {
        var userId = Guid.NewGuid();
        var token = RefreshToken.Create(new RefreshTokenCreationData(userId, "hash-123", 7));
        token.Id.Should().NotBeEmpty();
        token.UserId.Should().Be(userId);
        token.TokenHash.Should().Be("hash-123");
        token.IsUsed.Should().BeFalse();
        token.IsActive.Should().BeTrue();
    }

    [Fact]
    public void MarkAsUsed_Should_DeactivateAndRecordReplacementHash_When_Rotated()
    {
        var token = RefreshToken.Create(new RefreshTokenCreationData(Guid.NewGuid(), "old-hash", 7));
        token.MarkAsUsed("new-hash");
        token.IsUsed.Should().BeTrue();
        token.ReplacedByTokenHash.Should().Be("new-hash");
        token.RevokedAtUtc.Should().NotBeNull();
        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Revoke_Should_DeactivateToken_When_Called()
    {
        var token = RefreshToken.Create(new RefreshTokenCreationData(Guid.NewGuid(), "hash-1", 7));
        token.Revoke();
        token.RevokedAtUtc.Should().NotBeNull();
        token.IsActive.Should().BeFalse();
    }
}
