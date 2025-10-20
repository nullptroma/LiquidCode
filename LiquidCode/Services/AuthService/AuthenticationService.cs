using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LiquidCode.Extensions;
using LiquidCode.Models.Api.AuthenticationController;
using LiquidCode.Models.Constants;
using LiquidCode.Models.Database;
using LiquidCode.Repositories;
using LiquidCode.Tools;
using Microsoft.IdentityModel.Tokens;

namespace LiquidCode.Services.AuthService;

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

    public async Task<AuthTokensModel?> RegisterAsync(RegisterModel model, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if user already exists
            var userExists = await _userRepository.UserExistsAsync(model.Username, cancellationToken);
            if (userExists)
            {
                _logger.LogWarning("Registration attempt with existing username: {Username}", model.Username);
                return null;
            }

            // Generate password hash with salt
            var salt = StringTools.RandomBase64(AppConstants.PasswordSaltLength);
            var passwordHash = (model.Password + salt).ComputeSha256();

            // Create new user
            var newUser = new DbUser
            {
                Username = model.Username,
                Email = model.Email,
                Salt = salt,
                PassHash = passwordHash
            };

            await _userRepository.AddAsync(newUser, cancellationToken);
            _logger.LogInformation("User registered successfully: {Username}", model.Username);

            // Automatically log in the user
            return GenerateTokens(newUser.Username, newUser.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration: {Username}", model.Username);
            return null;
        }
    }

    public async Task<AuthTokensModel?> LoginAsync(
        LoginModel model, string userAgent, string ipAddress, CancellationToken cancellationToken = default)
    {
        try
        {
            // Find user by username
            var user = await _userRepository.FindByUsernameAsync(model.Username, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Login attempt for non-existent user: {Username}", model.Username);
                return null;
            }

            // Verify password
            var passwordHash = (model.Password + user.Salt).ComputeSha256();
            if (passwordHash != user.PassHash)
            {
                _logger.LogWarning("Invalid password for user: {Username}", model.Username);
                return null;
            }

            // Generate tokens and save refresh token
            var tokens = GenerateTokens(user.Username, user.Id);
            await SaveRefreshTokenAsync(user, tokens.RefreshToken, userAgent, ipAddress, cancellationToken);

            _logger.LogInformation("User logged in successfully: {Username}", model.Username);
            return tokens;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Username}", model.Username);
            return null;
        }
    }

    public async Task<AuthTokensModel?> RefreshAsync(
        RefreshTokenModel model, string userAgent, string ipAddress, CancellationToken cancellationToken = default)
    {
        try
        {
            // Find refresh token
            var refreshToken = await _userRepository.FindRefreshTokenAsync(model.RefreshToken, cancellationToken);
            if (refreshToken == null)
            {
                _logger.LogWarning("Refresh attempt with invalid token");
                return null;
            }

            // Check if token has expired
            if (DateTime.UtcNow > refreshToken.Expires)
            {
                _logger.LogWarning("Refresh token has expired for user: {UserId}", refreshToken.DbUser.Id);
                await _userRepository.RemoveRefreshTokenAsync(model.RefreshToken, cancellationToken);
                await _userRepository.SaveChangesAsync(cancellationToken);
                return null;
            }

            // Remove old refresh token
            await _userRepository.RemoveRefreshTokenAsync(model.RefreshToken, cancellationToken);

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

    private AuthTokensModel GenerateTokens(string username, int userId)
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
        var refreshToken = StringTools.RandomBase64(AppConstants.RefreshTokenLength);

        return new AuthTokensModel(token, refreshToken);
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
