using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Missions.Responses;

/// <summary>
/// Response model for a mission
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
    /// Maps database entity to response model
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
