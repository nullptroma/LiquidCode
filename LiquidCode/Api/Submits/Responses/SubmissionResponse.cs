using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Submits.Responses;

/// <summary>
/// Response model for a user submission
/// </summary>
public record SubmissionResponse(
    int Id,
    int UserId,
    SolutionResponse Solution
)
{
    /// <summary>
    /// Maps database entity to response model
    /// </summary>
    public static SubmissionResponse FromEntity(DbUserSubmit entity) => new(
        entity.Id,
        entity.User.Id,
        SolutionResponse.FromEntity(entity.Solution)
    );
}
