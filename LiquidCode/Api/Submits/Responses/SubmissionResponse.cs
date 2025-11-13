using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Submits.Responses;

/// <summary>
/// Модель ответа для отправки пользователя
/// </summary>
/// <param name="Id">Идентификатор отправки</param>
/// <param name="UserId">Идентификатор пользователя</param>
/// <param name="Solution">Решение пользователя с результатами тестирования</param>
/// <param name="ContestId">Идентификатор контеста (если отправка была в рамках контеста)</param>
/// <param name="ContestName">Название контеста (если отправка была в рамках контеста)</param>
/// <param name="SourceType">Контекст отправки (прямая отправка, контест с фиксированным окном или гибким окном)</param>
public record SubmissionResponse(
    int Id,
    int UserId,
    SolutionResponse Solution,
    int? ContestId,
    string? ContestName,
    SubmissionSourceType SourceType
)
{
    /// <summary>
    /// Отображает сущность базы данных на модель ответа
    /// </summary>
    public static SubmissionResponse FromEntity(DbUserSubmission entity) => new(
        entity.Id,
        entity.User.Id,
        SolutionResponse.FromEntity(entity.Solution),
        entity.ContestId,
        entity.Contest?.Name,
        entity.SourceType
    );
}
