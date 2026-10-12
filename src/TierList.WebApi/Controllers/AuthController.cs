using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TierList.Application.Features.Auth.Commands.Login;
using TierList.Application.Features.Auth.Commands.Logout;
using TierList.Application.Features.Auth.Commands.RefreshToken;
using TierList.Application.Features.Auth.Commands.Register;
using TierList.Application.Features.Auth.DTOs;

namespace TierList.WebApi.Controllers;

[Route("auth")]
public class AuthController : ApiControllerBase
{
    public record RefreshTokenRequest(string? RefreshToken);

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterCommand command)
    {
        var result = await Mediator.Send(command);
        SetRefreshTokenCookie(result.RefreshToken);

        return Ok(ToResponse(result));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginCommand command)
    {
        var result = await Mediator.Send(command);
        SetRefreshTokenCookie(result.RefreshToken);

        return Ok(ToResponse(result));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequest? request)
    {
        var refreshToken = request?.RefreshToken ?? Request.Cookies["refreshToken"];
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return BadRequest(new { title = "Bad Request", detail = "Refresh token is required." });
        }

        var result = await Mediator.Send(new RefreshTokenCommand(refreshToken));
        SetRefreshTokenCookie(result.RefreshToken);

        return Ok(ToResponse(result));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest? request)
    {
        var refreshToken = request?.RefreshToken ?? Request.Cookies["refreshToken"];
        await Mediator.Send(new LogoutCommand(refreshToken));

        Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict
        });

        return NoContent();
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(14)
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }

    private static AuthResponse ToResponse(AuthResultDto result) =>
        new(result.Id, result.UserName, result.AccessToken, result.ExpiresIn, result.Roles);

    public record AuthResponse(Guid Id, string UserName, string AccessToken, int ExpiresIn, IEnumerable<string> Roles);
}
