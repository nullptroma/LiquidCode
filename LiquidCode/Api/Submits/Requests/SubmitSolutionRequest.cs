using System.ComponentModel.DataAnnotations;

namespace LiquidCode.Api.Submits.Requests;

/// <summary>
/// Request model for submitting a solution
/// </summary>
public record SubmitSolutionRequest(
    [Required] int MissionId,
    [Required] [StringLength(16)] string Language,
    [Required] [StringLength(16)] string LanguageVersion,
    [Required] [StringLength(10000, MinimumLength = 1)] string SourceCode
);
