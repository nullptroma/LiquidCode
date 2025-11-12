using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Missions.Responses;

/// <summary>
/// Модель ответа для миссии с опциональной полной информацией о statement'ах
/// </summary>
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
