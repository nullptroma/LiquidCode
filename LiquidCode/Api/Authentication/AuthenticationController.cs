using LiquidCode.Api.Authentication.Requests;
using LiquidCode.Api.Authentication.Responses;
using LiquidCode.Domain.Interfaces.Services;
using LiquidCode.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LiquidCode.Api.Authentication;

/// <summary>
/// Контроллер аутентификации, обрабатывающий регистрацию пользователей, вход, обновление токенов и информацию о пользователе
/// </summary>
[Route("authentication")]
[ApiController]
public class AuthenticationController(IAuthenticationService authService) : ControllerBase
{
    /// <summary>
    /// Регистрирует нового пользователя
    /// </summary>
    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await authService.RegisterAsync(request, cancellationToken);
        if (result == null)
            return BadRequest("Registration failed. User may already exist.");

        return Ok(result);
    }

    /// <summary>
    /// Аутентифицирует пользователя с помощью имени пользователя и пароля
    /// </summary>
    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userAgent = Request.Headers.UserAgent.ToString();
        var ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

        var result = await authService.LoginAsync(request, userAgent, ipAddress, cancellationToken);
        if (result == null)
            return Unauthorized("Invalid username or password.");

        return Ok(result);
    }

    /// <summary>
    /// Обновляет истекший JWT токен с помощью токена обновления
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userAgent = Request.Headers.UserAgent.ToString();
        var ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

        var result = await authService.RefreshAsync(request, userAgent, ipAddress, cancellationToken);
        if (result == null)
            return Unauthorized("Token refresh failed. Token may have expired.");

        return Ok(result);
    }

    /// <summary>
    /// Получает имя пользователя текущего аутентифицированного пользователя
    /// </summary>
    [HttpGet("whoami")]
    [Authorize]
    public async Task<IActionResult> WhoAmI(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        var username = await authService.GetUsernameAsync(userId, cancellationToken);
        if (username == null)
            return NotFound("User not found.");

        return Ok(new WhoAmIResponse(username));
    }
}
