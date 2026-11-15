using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Common.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Web.API.Middlewares;

namespace Tests.UnitTests.Presentation.Middlewares;

public sealed class JwtBlacklistMiddlewareTests
{
    private readonly Mock<ITokenBlacklistService> _blacklistServiceMock = new();

    [Fact]
    public async Task InvokeAsync_Should_Return401Unauthorized_When_JtiIsBlacklisted()
    {
        var nextCalled = false;
        var middleware = new JwtBlacklistMiddleware(_ =>
        {
            nextCalled = true;

            return Task.CompletedTask;
        });
        var context = BuildAuthenticatedHttpContext("revoked-jti");
        _blacklistServiceMock.Setup(s => s.IsTokenBlacklistedAsync("revoked-jti", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        await middleware.InvokeAsync(context, _blacklistServiceMock.Object);
        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task InvokeAsync_Should_CallNext_When_JtiIsNotBlacklisted()
    {
        var nextCalled = false;
        var middleware = new JwtBlacklistMiddleware(_ =>
        {
            nextCalled = true;

            return Task.CompletedTask;
        });
        var context = BuildAuthenticatedHttpContext("active-jti");
        _blacklistServiceMock.Setup(s => s.IsTokenBlacklistedAsync("active-jti", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        await middleware.InvokeAsync(context, _blacklistServiceMock.Object);
        nextCalled.Should().BeTrue();
    }

    private static DefaultHttpContext BuildAuthenticatedHttpContext(string jti)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(JwtRegisteredClaimNames.Jti, jti)
        ], "Bearer"));

        return context;
    }
}
