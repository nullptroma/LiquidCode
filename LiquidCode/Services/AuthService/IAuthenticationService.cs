using LiquidCode.Models.Api.AuthenticationController;

namespace LiquidCode.Services.AuthService;

/// <summary>
/// Service interface for authentication operations
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Registers a new user
    /// </summary>
    /// <param name="model">Registration model with username, email, and password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication tokens (JWT and refresh token) or null if registration failed</returns>
    Task<AuthTokensModel?> RegisterAsync(RegisterModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user with username and password
    /// </summary>
    /// <param name="model">Login model with username and password</param>
    /// <param name="userAgent">User agent string (for token metadata)</param>
    /// <param name="ipAddress">IP address (for token metadata)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication tokens (JWT and refresh token) or null if login failed</returns>
    Task<AuthTokensModel?> LoginAsync(LoginModel model, string userAgent, string ipAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes an expired JWT token using a refresh token
    /// </summary>
    /// <param name="model">Refresh token model</param>
    /// <param name="userAgent">User agent string (for new token metadata)</param>
    /// <param name="ipAddress">IP address (for new token metadata)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New authentication tokens or null if refresh failed</returns>
    Task<AuthTokensModel?> RefreshAsync(RefreshTokenModel model, string userAgent, string ipAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the username of the currently authenticated user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Username or null if user not found</returns>
    Task<string?> GetUsernameAsync(int userId, CancellationToken cancellationToken = default);
}
