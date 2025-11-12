using LiquidCode.Domain.Enums;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Missions.Responses;

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
