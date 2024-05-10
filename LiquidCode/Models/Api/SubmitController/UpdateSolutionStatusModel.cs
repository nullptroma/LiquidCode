namespace LiquidCode.Models.Api.SubmitController;

public record UpdateSolutionStatusModel(string Status, int SubmissionId, int? TestCase, string? TimeUsed, int VerdictCode);