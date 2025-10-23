using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Domain.Interfaces.Repositories;

/// <summary>
/// Интерфейс репозитория для операций базы данных, связанных с миссиями
/// </summary>
public interface IMissionRepository : IRepository<DbMission>
{
    /// <summary>
    /// Получает миссии с пагинацией
    /// </summary>
    /// <param name="pageSize">Количество элементов на странице</param>
    /// <param name="pageNumber">Номер страницы (начиная с нуля)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Кортеж (миссии, естьСледующаяСтраница)</returns>
    Task<(IEnumerable<DbMission> Missions, bool HasNextPage)> GetMissionsPageAsync(
        int pageSize, int pageNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает миссии по автору
    /// </summary>
    Task<IEnumerable<DbMission>> GetMissionsByAuthorAsync(int authorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает текстовые данные миссии на определенном языке
    /// </summary>
    Task<DbMissionPublicTextData?> GetMissionTextAsync(int missionId, string language, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает все доступные языки для миссии
    /// </summary>
    Task<IEnumerable<string>> GetMissionLanguagesAsync(int missionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавляет текстовые данные миссии
    /// </summary>
    Task AddMissionTextAsync(DbMissionPublicTextData textData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавляет несколько записей текстовых данных миссии
    /// </summary>
    Task AddMissionTextsAsync(IEnumerable<DbMissionPublicTextData> textData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Подсчитывает общее количество миссий
    /// </summary>
    Task<int> CountMissionsAsync(CancellationToken cancellationToken = default);
}
