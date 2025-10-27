using LiquidCode.Api.Submits.Dto;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Submits.Responses;

/// <summary>
/// Модель ответа для решения
/// </summary>
public record SolutionResponse(
    int Id,
    int MissionId,
    string Language,
    string LanguageVersion,
    string SourceCode,
    string Status,
    DateTime Time,
    TesterState TesterState,
    TesterErrorCode TesterErrorCode,
    string? TesterMessage,
    int CurrentTest,
    int AmountOfTests
)
{
    /// <summary>
    /// Отображает сущность базы данных на модель ответа
    /// </summary>
    public static SolutionResponse FromEntity(DbSolution entity) => new(
        entity.Id,
        entity.Mission.Id,
        entity.Language,
        entity.LanguageVersion,
        entity.SourceCode,
        entity.Status,
        entity.Time,
        entity.TestingState,
        entity.TestingErrorCode,
        entity.TestingMessage,
        entity.CurrentTest,
        entity.AmountOfTests
    );
}
