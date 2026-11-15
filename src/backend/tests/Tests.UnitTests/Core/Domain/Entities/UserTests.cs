using Domain.Entities;
using FluentAssertions;

namespace Tests.UnitTests.Core.Domain.Entities;

public sealed class UserTests
{
    [Fact]
    public void Create_Should_NormalizeEmailAndSetActive_When_ValidDataProvided()
    {
        var roleId = Guid.NewGuid();
        var data = new UserCreationData("  Jane Doe  ", "  JANE.DOE@Example.COM ", "hashed-pw", roleId);
        var user = User.Create(data);
        user.Id.Should().NotBeEmpty();
        user.FullName.Should().Be("Jane Doe");
        user.Email.Should().Be("jane.doe@example.com");
        user.PasswordHash.Should().Be("hashed-pw");
        user.RoleId.Should().Be(roleId);
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void DeactivateAndActivate_Should_ToggleIsActiveState_When_Called()
    {
        var user = User.Create(new UserCreationData("User", "user@test.com", "hash", Guid.NewGuid()));
        user.Deactivate();
        user.IsActive.Should().BeFalse();
        user.Activate();
        user.IsActive.Should().BeTrue();
    }
}
