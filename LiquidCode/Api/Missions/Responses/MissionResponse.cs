using LiquidCode.Domain.Enums;
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
        includeStatements 
            ? entity.Statements?.Select(s => MissionStatementResponse.FromEntity(s)).ToList()
            : null
    );
}

/// <summary>
/// Модель statement'а миссии с текстами и медиа
/// </summary>
public record MissionStatementResponse(
    int Id,
    string Language,
    StatementFormat Format,
    Dictionary<string, string> StatementTexts,
    IReadOnlyList<MissionStatementMediaResponse> MediaFiles
)
{
    /// <summary>
    /// Отображает сущность базы данных на модель ответа
    /// </summary>
    public static MissionStatementResponse FromEntity(DbMissionStatement entity) => new(
        entity.Id,
        entity.Language,
        entity.Format,
        entity.StatementTexts,
        entity.MediaFiles
            .Select(m => MissionStatementMediaResponse.FromEntity(m))
            .ToList()
    );
}

/// <summary>
/// Модель медиа файла statement'а
/// </summary>
public record MissionStatementMediaResponse(
    int Id,
    string FileName,
    string MediaUrl
)
{
    /// <summary>
    /// Отображает сущность базы данных на модель ответа
    /// </summary>
    public static MissionStatementMediaResponse FromEntity(DbMissionStatementMedia entity) => new(
        entity.Id,
        entity.FileName,
        entity.MediaUrl
    );
}
