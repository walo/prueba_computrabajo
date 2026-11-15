// NOTA: Generar 1 archivo físico por clase:
// - Attributes/HasPermissionAttribute.cs
// - Middlewares/GlobalExceptionHandlingMiddleware.cs
// - Middlewares/JwtBlacklistMiddleware.cs
// - Middlewares/PermissionAuthorizationMiddleware.cs

using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Web.API.Middlewares;

public sealed class JwtBlacklistMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ITokenBlacklistService tokenBlacklistService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var jti = context.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            if (!string.IsNullOrWhiteSpace(jti) &&
                await tokenBlacklistService.IsTokenBlacklistedAsync(jti, context.RequestAborted))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/problem+json";

                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Auth.TokenRevoked",
                    Detail = "La sesión fue cerrada o el token de acceso ha sido revocado.",
                    Instance = context.Request.Path
                };
                problem.Extensions["traceId"] = context.TraceIdentifier;

                await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
                return;
            }
        }

        await next(context);
    }
}
