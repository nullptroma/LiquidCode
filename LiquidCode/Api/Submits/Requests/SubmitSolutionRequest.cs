using System.ComponentModel.DataAnnotations;

namespace LiquidCode.Api.Submits.Requests;

/// <summary>
/// Модель запроса для отправки решения
/// </summary>
public record SubmitSolutionRequest(
    [Required] int MissionId,
    [Required] [StringLength(16)] string Language,
    [Required] [StringLength(16)] string LanguageVersion,
    [Required] [StringLength(10000, MinimumLength = 1)] string SourceCode
);
