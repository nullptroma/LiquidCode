using LiquidCode.Api.Missions.Requests;
using LiquidCode.Api.Missions.Responses;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Domain.Services.Missions;

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
    /// Получает текстовые данные миссии на определенном языке
    /// </summary>
    /// <param name="missionId">ID миссии</param>
    /// <param name="language">Код языка</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Текстовые данные миссии в виде строки JSON или null, если не найдено</returns>
    Task<string?> GetMissionTextAsync(int missionId, string language, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает постраничный список миссий
    /// </summary>
    /// <param name="pageSize">Количество миссий на странице</param>
    /// <param name="pageNumber">Номер страницы, начиная с нуля</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список миссий с информацией о пагинации или null при недопустимых параметрах</returns>
    Task<MissionsPageResponse?> GetMissionsListAsync(int pageSize, int pageNumber, CancellationToken cancellationToken = default);
}
