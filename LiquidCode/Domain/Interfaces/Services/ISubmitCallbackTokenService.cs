using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Domain.Interfaces.Services;

/// <summary>
/// Сервис генерации и проверки токенов обратного вызова для отправок решений.
/// </summary>
public interface ISubmitCallbackTokenService
{
    /// <summary>
    /// Генерирует токен для обратного вызова на основе данных решения.
    /// </summary>
    string GenerateToken(DbSolution solution);

    /// <summary>
    /// Проверяет корректность предоставленного токена.
    /// </summary>
    bool ValidateToken(DbSolution solution, string providedToken);
}
