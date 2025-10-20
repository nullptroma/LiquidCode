using LiquidCode.Extensions;
using LiquidCode.Models.Api.AuthenticationController;
using LiquidCode.Services.AuthService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiquidCode.Controllers;

/// <summary>
/// Authentication controller handling user registration, login, token refresh, and user info
/// </summary>
[Route("[controller]")]
[ApiController]
public class AuthenticationController(IAuthenticationService authService) : ControllerBase
{
    /// <summary>
    /// Registers a new user
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await authService.RegisterAsync(model, cancellationToken);
        if (result == null)
            return BadRequest("Registration failed. User may already exist.");

        return Ok(result);
    }

    /// <summary>
    /// Authenticates a user with username and password
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userAgent = Request.Headers.UserAgent.ToString();
        var ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

        var result = await authService.LoginAsync(model, userAgent, ipAddress, cancellationToken);
        if (result == null)
            return Unauthorized("Invalid username or password.");

        return Ok(result);
    }

    /// <summary>
    /// Refreshes an expired JWT token using a refresh token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userAgent = Request.Headers.UserAgent.ToString();
        var ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

        var result = await authService.RefreshAsync(model, userAgent, ipAddress, cancellationToken);
        if (result == null)
            return Unauthorized("Token refresh failed. Token may have expired.");

        return Ok(result);
    }

    /// <summary>
    /// Gets the current authenticated user's username
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

        return Ok(new { username });
    }
}