using LiquidCode.Api.Missions.Requests;
using LiquidCode.Api.Missions.Responses;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Domain.Interfaces.Services;

/// <summary>
/// Интерфейс сервиса для операций, связанных с миссиями
/// </summary>
public interface IMissionService
{
    /// <summary>
    /// Загружает новую миссию из ZIP файла
    /// </summary>
    /// <param name="form">Форма загрузки с файлом миссии и метаданными</param>
    /// <param name="userId">ID пользователя, загружающего миссию</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Созданная модель миссии или null, если загрузка не удалась</returns>
    Task<MissionResponse?> UploadMissionAsync(UploadMissionRequest form, int userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает постраничный список миссий
    /// </summary>
    /// <param name="pageSize">Количество миссий на странице</param>
    /// <param name="pageNumber">Номер страницы, начиная с нуля</param>
    /// <param name="tags">Список тегов для фильтрации (опционально)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список миссий с информацией о пагинации или null при недопустимых параметрах</returns>
    Task<MissionsPageResponse?> GetMissionsListAsync(
        int pageSize,
        int pageNumber,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает подробную информацию о миссии
    /// </summary>
    Task<MissionResponse?> GetMissionAsync(int missionId, CancellationToken cancellationToken = default);
}
