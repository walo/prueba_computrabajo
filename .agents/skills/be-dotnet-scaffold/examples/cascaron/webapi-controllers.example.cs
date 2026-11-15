// NOTA DE ARQUITECTURA: Al generar el proyecto real, cada controlador DEBE crearse en su propio archivo físico:
// - Controllers/ApiController.cs
// - Controllers/AuthController.cs
// - Controllers/UsersController.cs

using System.IdentityModel.Tokens.Jwt;
using Application.Common.Interfaces;
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.Logout;
using Application.Features.Auth.Commands.RefreshToken;
using Application.Features.Auth.DTOs;
using Application.Features.Auth.Queries.GetCurrentUser;
using Application.Features.Users.Queries.GetUsers;
using Domain.Constants;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web.API.Attributes;

namespace Web.API.Controllers;

// --- Archivo: Controllers/ApiController.cs ---
[ApiController]
[Route("api/v1/[controller]")]
public abstract class ApiController : ControllerBase
{
    protected IActionResult HandleFailure(List<Error> errors)
    {
        if (errors.Count == 0) return Problem();

        if (errors.All(e => e.Type == ErrorType.Validation))
        {
            var validationErrors = errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

            return BadRequest(new ValidationProblemDetails(validationErrors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation.Failed"
            });
        }

        var first = errors[0];
        return first.Type switch
        {
            ErrorType.NotFound => NotFound(new ProblemDetails { Status = 404, Title = first.Code, Detail = first.Description }),
            ErrorType.Unauthorized => Unauthorized(new ProblemDetails { Status = 401, Title = first.Code, Detail = first.Description }),
            ErrorType.Forbidden => StatusCode(403, new ProblemDetails { Status = 403, Title = first.Code, Detail = first.Description }),
            ErrorType.Conflict => Conflict(new ProblemDetails { Status = 409, Title = first.Code, Detail = first.Description }),
            _ => StatusCode(500, new ProblemDetails { Status = 500, Title = "Server.UnexpectedError", Detail = first.Description })
        };
    }
}

// --- Archivo: Controllers/AuthController.cs ---
[Route("api/v1/auth")]
public sealed class AuthController(ISender sender, ICurrentUserContext currentUser) : ApiController
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new LoginCommand(request.Email, request.Password), ct);
        return result.Match(Ok, HandleFailure);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new RefreshTokenCommand(request.RefreshToken), ct);
        return result.Match(Ok, HandleFailure);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        if (currentUser.UserId is null) return Unauthorized();

        var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value ?? string.Empty;
        var expClaim = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
        var expiresAtUtc = long.TryParse(expClaim, out var expSeconds)
            ? DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime
            : DateTime.UtcNow.AddMinutes(15);

        var result = await sender.Send(new LogoutCommand(currentUser.UserId.Value, jti, expiresAtUtc), ct);
        return result.Match(_ => NoContent(), HandleFailure);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        if (currentUser.UserId is null) return Unauthorized();

        var result = await sender.Send(new GetCurrentUserQuery(currentUser.UserId.Value), ct);
        return result.Match(Ok, HandleFailure);
    }
}

// --- Archivo: Controllers/UsersController.cs ---
[Route("api/v1/users")]
public sealed class UsersController(ISender sender) : ApiController
{
    [HttpGet]
    [HasPermission(Permissions.Users.Read)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await sender.Send(new GetUsersQuery(), ct);
        return result.Match(Ok, HandleFailure);
    }
}
