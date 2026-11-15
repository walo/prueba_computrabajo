using System.IdentityModel.Tokens.Jwt;
using Application.Common.Interfaces;
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.Logout;
using Application.Features.Auth.Commands.RefreshToken;
using Application.Features.Auth.DTOs;
using Application.Features.Auth.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.API.Controllers;

[Route("api/v1/auth")]
public sealed class AuthController(
    ISender sender,
    ICurrentUserContext currentUser) : ApiController
{
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthTokensResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new LoginCommand(request.Email, request.Password), ct);

        return result.Match(Ok, HandleFailure);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthTokensResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new RefreshTokenCommand(request.RefreshToken), ct);

        return result.Match(Ok, HandleFailure);
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        if (currentUser.UserId is null)
        {
            return Unauthorized();
        }
        var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value ?? string.Empty;
        var expiresAtUtc = ResolveTokenExpirationUtc();
        var result = await sender.Send(new LogoutCommand(currentUser.UserId.Value, jti, expiresAtUtc), ct);

        return result.Match(_ => NoContent(), HandleFailure);
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        if (currentUser.UserId is null)
        {
            return Unauthorized();
        }
        var result = await sender.Send(new GetCurrentUserQuery(currentUser.UserId.Value), ct);

        return result.Match(Ok, HandleFailure);
    }

    private DateTime ResolveTokenExpirationUtc()
    {
        var expClaim = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
        if (long.TryParse(expClaim, out var expSeconds))
        {
            return DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;
        }

        return DateTime.UtcNow.AddMinutes(15);
    }
}
