using System.ComponentModel.DataAnnotations;

namespace LiquidCode.Api.Submits.Requests;

/// <summary>
/// Модель запроса для обновления статуса решения (вызывается модулем тестирования)
/// </summary>
public record UpdateSolutionStatusRequest(
    [Required] int SubmissionId,
    [Required] int VerdictCode,
    int? TestCase = null,
    string? TimeUsed = null
);
