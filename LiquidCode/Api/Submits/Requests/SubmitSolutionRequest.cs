using System.ComponentModel.DataAnnotations;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Submits.Requests;

/// <summary>
/// Модель запроса для отправки решения
/// </summary>
/// <param name="MissionId">Идентификатор миссии</param>
/// <param name="Language">Язык программирования</param>
/// <param name="LanguageVersion">Версия языка программирования</param>
/// <param name="SourceCode">Исходный код решения</param>
/// <param name="ContestId">Идентификатор контеста (если решение отправляется в рамках контеста)</param>
public record SubmitSolutionRequest(
    [Required] int MissionId,
    [Required] [StringLength(16)] string Language,
    [Required] [StringLength(16)] string LanguageVersion,
    [Required] [StringLength(10000, MinimumLength = 1)] string SourceCode,
    int? ContestId);
