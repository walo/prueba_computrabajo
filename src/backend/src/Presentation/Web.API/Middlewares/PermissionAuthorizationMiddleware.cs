using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Web.API.Attributes;

namespace Web.API.Middlewares;

public sealed class PermissionAuthorizationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IPermissionCacheService permissionCacheService)
    {
        var requiredPermissions = context.GetEndpoint()?.Metadata.GetOrderedMetadata<HasPermissionAttribute>();
        if (requiredPermissions is null || requiredPermissions.Count == 0)
        {
            await next(context);

            return;
        }
        var userId = ResolveUserId(context);
        if (userId is null || !await HasAllPermissionsAsync(permissionCacheService, userId.Value, requiredPermissions, context.RequestAborted))
        {
            await WriteForbiddenResponseAsync(context);

            return;
        }
        await next(context);
    }

    private static Guid? ResolveUserId(HttpContext context)
    {
        var rawUserId = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(rawUserId, out var userId) ? userId : null;
    }

    private static async Task<bool> HasAllPermissionsAsync(
        IPermissionCacheService permissionCacheService,
        Guid userId,
        IReadOnlyList<HasPermissionAttribute> requiredPermissions,
        CancellationToken ct)
    {
        var userPermissions = await permissionCacheService.GetPermissionsForUserAsync(userId, ct);

        return requiredPermissions.All(attribute => userPermissions.Contains(attribute.Permission));
    }

    private static async Task WriteForbiddenResponseAsync(HttpContext context)
    {
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Auth.Forbidden",
            Detail = "You do not have the required permission to perform this action.",
            Instance = context.Request.Path
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
