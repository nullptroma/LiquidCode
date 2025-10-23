using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Missions.Responses;

/// <summary>
/// Модель ответа для миссии
/// </summary>
public record MissionResponse(
    int Id, 
    int AuthorId, 
    string Name, 
    int Difficulty, 
    DateTime CreatedAt, 
    DateTime UpdatedAt
)
{
    /// <summary>
    /// Отображает сущность базы данных на модель ответа
    /// </summary>
    public static MissionResponse FromEntity(DbMission entity) => new(
        entity.Id,
        entity.Author.Id,
        entity.Name,
        entity.Difficulty,
        entity.CreatedAt,
        entity.UpdatedAt
    );
}
