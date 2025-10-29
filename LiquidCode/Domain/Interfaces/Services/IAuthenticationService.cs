using LiquidCode.Api.Authentication.Requests;
using LiquidCode.Api.Authentication.Responses;

namespace LiquidCode.Domain.Interfaces.Services;

/// <summary>
/// Интерфейс сервиса для операций аутентификации
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Регистрирует нового пользователя
    /// </summary>
    Task<AuthTokensResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Аутентифицирует пользователя с помощью имени пользователя и пароля
    /// </summary>
    Task<AuthTokensResponse?> LoginAsync(LoginRequest request, string userAgent, string ipAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновляет истекший JWT токен с помощью токена обновления
    /// </summary>
    Task<AuthTokensResponse?> RefreshAsync(RefreshTokenRequest request, string userAgent, string ipAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает имя пользователя текущего аутентифицированного пользователя
    /// </summary>
    Task<string?> GetUsernameAsync(int userId, CancellationToken cancellationToken = default);
}
