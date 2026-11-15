using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.API.Middlewares;

public sealed class JwtBlacklistMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ITokenBlacklistService tokenBlacklistService)
    {
        var jti = ResolveAuthenticatedJti(context);
        if (!string.IsNullOrWhiteSpace(jti) &&
            await tokenBlacklistService.IsTokenBlacklistedAsync(jti, context.RequestAborted))
        {
            await WriteRevokedResponseAsync(context);

            return;
        }
        await next(context);
    }

    private static string? ResolveAuthenticatedJti(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return context.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
    }

    private static async Task WriteRevokedResponseAsync(HttpContext context)
    {
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Auth.TokenRevoked",
            Detail = "The session has been closed or the access token has been revoked.",
            Instance = context.Request.Path
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
