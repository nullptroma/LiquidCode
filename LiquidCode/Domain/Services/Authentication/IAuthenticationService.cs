using LiquidCode.Api.Authentication.Requests;
using LiquidCode.Api.Authentication.Responses;

namespace LiquidCode.Domain.Services.Authentication;

/// <summary>
/// Service interface for authentication operations
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Registers a new user
    /// </summary>
    Task<AuthTokensResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user with username and password
    /// </summary>
    Task<AuthTokensResponse?> LoginAsync(LoginRequest request, string userAgent, string ipAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes an expired JWT token using a refresh token
    /// </summary>
    Task<AuthTokensResponse?> RefreshAsync(RefreshTokenRequest request, string userAgent, string ipAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the username of the currently authenticated user
    /// </summary>
    Task<string?> GetUsernameAsync(int userId, CancellationToken cancellationToken = default);
}
