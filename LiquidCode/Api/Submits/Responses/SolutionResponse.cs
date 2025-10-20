using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Submits.Responses;

/// <summary>
/// Response model for a solution
/// </summary>
public record SolutionResponse(
    int Id,
    int MissionId,
    string Language,
    string LanguageVersion,
    string SourceCode,
    string Status,
    DateTime Time
)
{
    /// <summary>
    /// Maps database entity to response model
    /// </summary>
    public static SolutionResponse FromEntity(DbSolution entity) => new(
        entity.Id,
        entity.Mission.Id,
        entity.Language,
        entity.LanguageVersion,
        entity.SourceCode,
        entity.Status,
        entity.Time
    );
}
