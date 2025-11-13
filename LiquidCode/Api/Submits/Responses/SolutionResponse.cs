using LiquidCode.Api.Submits.Dto;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Submits.Responses;

/// <summary>
/// Модель ответа для решения
/// </summary>
/// <param name="Id">Идентификатор решения</param>
/// <param name="MissionId">Идентификатор миссии</param>
/// <param name="Language">Язык программирования</param>
/// <param name="LanguageVersion">Версия языка программирования</param>
/// <param name="SourceCode">Исходный код решения</param>
/// <param name="Status">Статус решения</param>
/// <param name="Time">Дата и время отправки</param>
/// <param name="TesterState">Состояние тестирования</param>
/// <param name="TesterErrorCode">Код ошибки тестирования</param>
/// <param name="TesterMessage">Сообщение от тестирующего модуля</param>
/// <param name="CurrentTest">Номер текущего теста</param>
/// <param name="AmountOfTests">Общее количество тестов</param>
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
