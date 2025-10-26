using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Submits.Responses;

/// <summary>
/// Модель ответа для отправки пользователя
/// </summary>
public record SubmissionResponse(
    int Id,
    int UserId,
    SolutionResponse Solution
)
{
    /// <summary>
    /// Отображает сущность базы данных на модель ответа
    /// </summary>
    public static SubmissionResponse FromEntity(DbUserSubmission entity) => new(
        entity.Id,
        entity.User.Id,
        SolutionResponse.FromEntity(entity.Solution)
    );
}
