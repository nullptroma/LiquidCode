namespace LiquidCode.Models.Api.SubmitController;

public record SolutionSubmitModel(int MissionId, string Language, string LanguageVersion, string SourceCode);