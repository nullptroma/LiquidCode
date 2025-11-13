namespace LiquidCode.Api.Profile.Responses;

/// <summary>
/// Детальная информация о профиле пользователя
/// </summary>
/// <param name="Header">Основная информация профиля</param>
/// <param name="Problems">Прогресс решения задач</param>
/// <param name="Competencies">Компетенции по тегам</param>
/// <param name="RecentSubmissions">Последние отправленные решения</param>
/// <param name="AuthoredMissions">Созданные пользователем миссии</param>
/// <param name="Activity">Активность пользователя</param>
public record ProfileDetailsResponse(
    ProfileHeaderResponse Header,
    ProfileProblemProgressResponse Problems,
    IReadOnlyList<ProfileCompetencyResponse> Competencies,
    IReadOnlyList<ProfileMissionActivityItemResponse> RecentSubmissions,
    IReadOnlyList<ProfileAuthoredMissionResponse> AuthoredMissions,
    ProfileActivityResponse Activity);
