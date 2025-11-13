using LiquidCode.Domain.Enums;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Missions.Responses;

/// <summary>
/// Модель statement'а миссии с текстами и медиа
/// </summary>
/// <param name="Id">Идентификатор statement'а</param>
/// <param name="Language">Код языка statement'а (например, "ru", "en")</param>
/// <param name="Format">Формат statement'а (Markdown, HTML и т.д.)</param>
/// <param name="StatementTexts">Словарь текстов statement'а (ключ - секция, значение - текст)</param>
/// <param name="MediaFiles">Список медиа-файлов, связанных со statement'ом</param>
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
