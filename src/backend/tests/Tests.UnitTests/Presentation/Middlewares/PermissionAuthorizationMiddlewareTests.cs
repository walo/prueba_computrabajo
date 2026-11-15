using System.Security.Claims;
using Application.Common.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Web.API.Attributes;
using Web.API.Middlewares;

namespace Tests.UnitTests.Presentation.Middlewares;

public sealed class PermissionAuthorizationMiddlewareTests
{
    private readonly Mock<IPermissionCacheService> _permissionCacheMock = new();

    [Fact]
    public async Task InvokeAsync_Should_Return403Forbidden_When_UserLacksPermission()
    {
        var userId = Guid.NewGuid();
        var nextCalled = false;
        var middleware = new PermissionAuthorizationMiddleware(_ =>
        {
            nextCalled = true;

            return Task.CompletedTask;
        });
        var context = BuildContextWithPermission(userId, "Users.Read");
        _permissionCacheMock.Setup(s => s.GetPermissionsForUserAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(["Products.Read"]);
        await middleware.InvokeAsync(context, _permissionCacheMock.Object);
        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task InvokeAsync_Should_CallNext_When_UserHasPermission()
    {
        var userId = Guid.NewGuid();
        var nextCalled = false;
        var middleware = new PermissionAuthorizationMiddleware(_ =>
        {
            nextCalled = true;

            return Task.CompletedTask;
        });
        var context = BuildContextWithPermission(userId, "Users.Read");
        _permissionCacheMock.Setup(s => s.GetPermissionsForUserAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(["Users.Read"]);
        await middleware.InvokeAsync(context, _permissionCacheMock.Object);
        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    private static DefaultHttpContext BuildContextWithPermission(Guid userId, string permission)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        ], "Bearer"));
        var endpoint = new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(new HasPermissionAttribute(permission)), "TestEndpoint");
        context.SetEndpoint(endpoint);

        return context;
    }
}
