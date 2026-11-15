using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Features.Auth.DTOs;
using FluentAssertions;
using Tests.IntegrationTests.Common;

namespace Tests.IntegrationTests.Features.Auth;

public sealed class AuthFlowIntegrationTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task HealthEndpoint_Should_Return200Ok_When_Called()
    {
        var response = await Client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetMe_Should_ReturnCurrentUserWithPermissions_When_Authenticated()
    {
        await AuthenticateAsAdminAsync();
        var response = await Client.GetAsync("/api/v1/auth/me");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var currentUser = await response.Content.ReadFromJsonAsync<CurrentUserResponse>();
        currentUser.Should().NotBeNull();
        currentUser!.Email.Should().Be("admin@enterprise.local");
        currentUser.Permissions.Should().Contain("Users.Read");
    }

    [Fact]
    public async Task Refresh_Should_RotateAndRejectReuse_When_OldRefreshTokenIsReused()
    {
        var initialTokens = await AuthenticateAsAdminAsync();
        var refreshResponse = await Client.PostAsJsonAsync("/api/v1/auth/refresh", new RefreshTokenRequest(initialTokens.RefreshToken));
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var rotatedTokens = await refreshResponse.Content.ReadFromJsonAsync<AuthTokensResponse>();
        rotatedTokens!.RefreshToken.Should().NotBe(initialTokens.RefreshToken);
        var reuseResponse = await Client.PostAsJsonAsync("/api/v1/auth/refresh", new RefreshTokenRequest(initialTokens.RefreshToken));
        reuseResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var invalidatedSessionResponse = await Client.PostAsJsonAsync("/api/v1/auth/refresh", new RefreshTokenRequest(rotatedTokens.RefreshToken));
        invalidatedSessionResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_Should_BlacklistAccessToken_When_LoggedOut()
    {
        var tokens = await AuthenticateAsAdminAsync();
        var logoutResponse = await Client.PostAsync("/api/v1/auth/logout", null);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        var meAfterLogout = await Client.GetAsync("/api/v1/auth/me");
        meAfterLogout.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UsersEndpoint_Should_Return200ForAdminAnd403ForStandardUser()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        var anonymousResponse = await Client.GetAsync("/api/v1/users");
        anonymousResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        await AuthenticateAsUserAsync();
        var forbiddenResponse = await Client.GetAsync("/api/v1/users");
        forbiddenResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        await AuthenticateAsAdminAsync();
        var authorizedResponse = await Client.GetAsync("/api/v1/users");
        authorizedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
