using System.ComponentModel.DataAnnotations;
using LiquidCode.Infrastructure.Database.Entities;
using LiquidCode.Shared.Validation;

namespace LiquidCode.Api.Submits.Requests;

/// <summary>
/// Модель запроса для отправки решения
/// </summary>
/// <param name="MissionId">Идентификатор миссии</param>
/// <param name="Language">Язык программирования</param>
/// <param name="LanguageVersion">Версия языка программирования</param>
/// <param name="SourceCode">Исходный код решения</param>
/// <param name="ContestAttemptId">Идентификатор попытки контеста (если решение отправляется в рамках контеста)</param>
public record SubmitSolutionRequest(
    [Required] int MissionId,
    [Required] [StringLength(ValidationLengths.Solution.LanguageMax)] string Language,
    [Required] [StringLength(ValidationLengths.Solution.LanguageVersionMax)] string LanguageVersion,
    [Required] [StringLength(ValidationLengths.Solution.SourceCodeMax, MinimumLength = ValidationLengths.Solution.SourceCodeMin)] string SourceCode,
    int? ContestAttemptId);
