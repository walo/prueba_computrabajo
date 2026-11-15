using System.Security.Claims;
using Application.Abstractions.Caching;
using Application.Attributes;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Moq;
using Web.API.Middlewares;
using Xunit;

namespace Tests.WebApi.Middlewares;

public sealed class AuthorizationMiddlewareTests
{
    private readonly Mock<IPermissionCacheService> _permissionCacheServiceMock;
    private readonly AuthorizationMiddleware _middleware;

    public AuthorizationMiddlewareTests()
    {
        _permissionCacheServiceMock = new Mock<IPermissionCacheService>();
        _middleware = new AuthorizationMiddleware(_permissionCacheServiceMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_Should_Return403Forbidden_When_UserLacksRequiredPermission()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var requiredPermission = "Features.Create";

        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        ], "TestAuth"));

        var endpoint = new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(new RequirePermissionAttribute(requiredPermission)),
            "TestEndpoint");

        context.SetEndpoint(endpoint);

        _permissionCacheServiceMock
            .Setup(s => s.HasPermissionAsync(userId, requiredPermission, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _middleware.InvokeAsync(context, _ => Task.CompletedTask);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task InvokeAsync_Should_CallNext_When_UserHasRequiredPermission()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var requiredPermission = "Features.Read";

        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        ], "TestAuth"));

        var endpoint = new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(new RequirePermissionAttribute(requiredPermission)),
            "TestEndpoint");

        context.SetEndpoint(endpoint);

        _permissionCacheServiceMock
            .Setup(s => s.HasPermissionAsync(userId, requiredPermission, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var nextCalled = false;

        // Act
        await _middleware.InvokeAsync(context, _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        // Assert
        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }
}
