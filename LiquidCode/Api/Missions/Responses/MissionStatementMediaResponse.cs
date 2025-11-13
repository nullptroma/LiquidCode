using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Missions.Responses;

/// <summary>
/// Модель медиа файла statement'а
/// </summary>
/// <param name="Id">Идентификатор медиа-файла</param>
/// <param name="FileName">Имя файла</param>
/// <param name="MediaUrl">URL медиа-файла</param>
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
