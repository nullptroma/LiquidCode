using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Domain.Interfaces.Repositories;

/// <summary>
/// Интерфейс репозитория для операций базы данных, связанных с миссиями
/// </summary>
public interface IMissionRepository : IRepository<DbMission>
{

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
    Task CreateMissionTextAsync(DbMissionPublicTextData textData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавляет несколько записей текстовых данных миссии
    /// </summary>
    Task CreateMissionTextsAsync(IEnumerable<DbMissionPublicTextData> textData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Подсчитывает общее количество миссий
    /// </summary>
    Task<int> CountMissionsAsync(CancellationToken cancellationToken = default);
}
