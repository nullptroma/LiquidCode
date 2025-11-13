namespace LiquidCode.Api.Profile.Responses;

/// <summary>
/// Компетенция пользователя по определенному тегу
/// </summary>
/// <param name="TagName">Название тега</param>
/// <param name="SolvedMissions">Количество решенных миссий с этим тегом</param>
/// <param name="TotalMissions">Общее количество миссий с этим тегом</param>
public record ProfileCompetencyResponse(
    string TagName,
    int SolvedMissions,
    int TotalMissions);
