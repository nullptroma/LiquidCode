using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Missions.Responses;

/// <summary>
/// Модель ответа для миссии с опциональной полной информацией о statement'ах
/// </summary>
/// <param name="Id">Идентификатор миссии</param>
/// <param name="AuthorId">Идентификатор автора</param>
/// <param name="Name">Название миссии</param>
/// <param name="Difficulty">Уровень сложности (числовое значение рейтинга)</param>
/// <param name="Tags">Список тегов миссии, отсортированный по алфавиту</param>
/// <param name="CreatedAt">Дата и время создания</param>
/// <param name="UpdatedAt">Дата и время последнего обновления</param>
/// <param name="TimeLimitMilliseconds">Ограничение по времени выполнения в миллисекундах</param>
/// <param name="MemoryLimitBytes">Ограничение по использованию памяти в байтах</param>
/// <param name="Statements">Список statement'ов миссии на разных языках (может быть null при получении списков)</param>
public record MissionResponse(
    int Id,
    int AuthorId,
    string Name,
    int Difficulty,
    IReadOnlyList<string> Tags,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int? TimeLimitMilliseconds,
    int? MemoryLimitBytes,
    // Дополнительные данные (могут быть null при получении списков)
    IReadOnlyList<MissionStatementResponse>? Statements = null
)
{
    /// <summary>
    /// Отображает сущность базы данных на модель ответа
    /// </summary>
    public static MissionResponse FromEntity(DbMission entity, bool includeStatements = false) => new(
        entity.Id,
        entity.Author.Id,
        entity.Name,
        entity.Difficulty,
        entity.MissionTags.Select(mt => mt.Tag.Name).Distinct().OrderBy(name => name).ToList(),
        entity.CreatedAt,
        entity.UpdatedAt,
        entity.TimeLimitMilliseconds,
        entity.MemoryLimitBytes,
        includeStatements 
            ? entity.Statements?.Select(s => MissionStatementResponse.FromEntity(s)).ToList()
            : null
    );
}
