using LiquidCode.Models.Database;

namespace LiquidCode.Models.Api.SubmitController;

public record SolutionInfoModel(
    int MissionId,
    string Language,
    string LanguageVersion,
    string SourceCode,
    string Status,
    DateTime Time);