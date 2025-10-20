using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using LiquidCode.Api.Authentication.Requests;
using LiquidCode.Api.Authentication.Responses;
using LiquidCode.Domain.Interfaces.Repositories;
using LiquidCode.Infrastructure.Database.Entities;
using LiquidCode.Shared.Constants;
using LiquidCode.Shared.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace LiquidCode.Domain.Services.Authentication;

/// <summary>
/// Service implementation for authentication operations
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        IConfiguration configuration,
        IUserRepository userRepository,
        ILogger<AuthenticationService> logger)
    {
        _configuration = configuration;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<AuthTokensResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if user already exists
            var userExists = await _userRepository.UserExistsAsync(request.Username, cancellationToken);
            if (userExists)
            {
                _logger.LogWarning("Registration attempt with existing username: {Username}", request.Username);
                return null;
            }

            // Generate secure password hash using BCrypt
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, AppConstants.BcryptWorkFactor);

            // Create new user (Salt is now handled internally by BCrypt)
            var newUser = new DbUser
            {
                Username = request.Username,
                Email = request.Email,
                PassHash = passwordHash,
                Salt = "" // BCrypt manages salt internally
            };

            await _userRepository.AddAsync(newUser, cancellationToken);
            _logger.LogInformation("User registered successfully: {Username}", request.Username);

            // Automatically log in the user
            return GenerateTokens(newUser.Username, newUser.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration: {Username}", request.Username);
            return null;
        }
    }

    public async Task<AuthTokensResponse?> LoginAsync(
        LoginRequest request, string userAgent, string ipAddress, CancellationToken cancellationToken = default)
    {
        try
        {
            // Find user by username
            var user = await _userRepository.FindByUsernameAsync(request.Username, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Login attempt for non-existent user: {Username}", request.Username);
                return null;
            }

            // Verify password using BCrypt
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PassHash))
            {
                _logger.LogWarning("Invalid password for user: {Username}", request.Username);
                return null;
            }

            // Generate tokens and save refresh token
            var tokens = GenerateTokens(user.Username, user.Id);
            await SaveRefreshTokenAsync(user, tokens.RefreshToken, userAgent, ipAddress, cancellationToken);

            _logger.LogInformation("User logged in successfully: {Username}", request.Username);
            return tokens;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Username}", request.Username);
            return null;
        }
    }

    public async Task<AuthTokensResponse?> RefreshAsync(
        RefreshTokenRequest request, string userAgent, string ipAddress, CancellationToken cancellationToken = default)
    {
        try
        {
            // Find refresh token
            var refreshToken = await _userRepository.FindRefreshTokenAsync(request.RefreshToken, cancellationToken);
            if (refreshToken == null)
            {
                _logger.LogWarning("Refresh attempt with invalid token");
                return null;
            }

            // Check if token has expired
            if (DateTime.UtcNow > refreshToken.Expires)
            {
                _logger.LogWarning("Refresh token has expired for user: {UserId}", refreshToken.DbUser.Id);
                await _userRepository.RemoveRefreshTokenAsync(request.RefreshToken, cancellationToken);
                await _userRepository.SaveChangesAsync(cancellationToken);
                return null;
            }

            // Remove old refresh token
            await _userRepository.RemoveRefreshTokenAsync(request.RefreshToken, cancellationToken);

            // Generate new tokens
            var newTokens = GenerateTokens(refreshToken.DbUser.Username, refreshToken.DbUser.Id);
            await SaveRefreshTokenAsync(refreshToken.DbUser, newTokens.RefreshToken, userAgent, ipAddress, cancellationToken);

            _logger.LogInformation("Tokens refreshed for user: {UserId}", refreshToken.DbUser.Id);
            return newTokens;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return null;
        }
    }

    public async Task<string?> GetUsernameAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.FindByIdAsync(userId, cancellationToken);
            return user?.Username;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting username for user: {UserId}", userId);
            return null;
        }
    }

    private AuthTokensResponse GenerateTokens(string username, int userId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username),
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration[ConfigurationKeys.JwtSigningKey] ?? throw new InvalidOperationException("JWT signing key not configured")));

        var jwt = new JwtSecurityToken(
            issuer: _configuration[ConfigurationKeys.JwtIssuer],
            audience: _configuration[ConfigurationKeys.JwtAudience],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(AppConstants.JwtExpirationMinutes),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        var refreshToken = StringExtensions.RandomBase64(AppConstants.RefreshTokenLength);

        return new AuthTokensResponse(token, refreshToken);
    }

    private async Task SaveRefreshTokenAsync(
        DbUser user, string refreshToken, string userAgent, string ipAddress, CancellationToken cancellationToken)
    {
        // Check and cleanup old tokens if needed
        var tokenCount = await _userRepository.GetRefreshTokenCountAsync(user.Id, cancellationToken);
        if (tokenCount >= AppConstants.MaxRefreshTokensPerUser)
        {
            var oldestToken = await _userRepository.GetOldestRefreshTokenAsync(user.Id, cancellationToken);
            if (oldestToken != null)
            {
                await _userRepository.RemoveRefreshTokenAsync(oldestToken.Token, cancellationToken);
            }
        }

        // Create and save new refresh token
        var newRefreshToken = new DbRefreshToken
        {
            Token = refreshToken,
            DbUser = user,
            Expires = DateTime.UtcNow.AddDays(AppConstants.RefreshTokenExpirationDays),
            OsName = userAgent[..Math.Min(512, userAgent.Length)],
            IpAddress = ipAddress
        };

        await _userRepository.AddRefreshTokenAsync(newRefreshToken, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
    }
}
