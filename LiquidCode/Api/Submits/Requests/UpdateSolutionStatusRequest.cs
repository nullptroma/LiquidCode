using System.ComponentModel.DataAnnotations;

namespace LiquidCode.Api.Submits.Requests;

/// <summary>
/// Request model for updating solution status (called by testing module)
/// </summary>
public record UpdateSolutionStatusRequest(
    [Required] int SubmissionId,
    [Required] int VerdictCode,
    int? TestCase = null,
    string? TimeUsed = null
);
