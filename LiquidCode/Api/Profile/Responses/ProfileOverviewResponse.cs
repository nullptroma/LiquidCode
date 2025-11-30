namespace LiquidCode.Api.Profile.Responses;

/// <summary>
/// Ответ с общей информацией профиля
/// </summary>
/// <param name="Identity">Основная информация об аккаунте</param>
/// <param name="Solutions">Статистика по решенным задачам</param>
/// <param name="Contests">Статистика участия в контестах</param>
/// <param name="Creation">Статистика созданного контента</param>
public record ProfileOverviewResponse(
    ProfileIdentityResponse Identity,
    ProfileSolutionsStatsResponse Solutions,
    ProfileContestStatsResponse Contests,
    ProfileCreationStatsResponse Creation);

public record ProfileIdentityResponse(
    int UserId,
    string Username,
    string Email,
    DateTime CreatedAt);

public record ProfileSolutionsStatsResponse(int TotalSolved, int SolvedLast7Days);

public record ProfileContestStatsResponse(int TotalParticipations, int ParticipationsLast7Days);

public record ProfileCreationStatsResponse(
    ProfileMetricResponse Missions,
    ProfileMetricResponse Contests,
    ProfileMetricResponse Articles);

public record ProfileMetricResponse(int Total, int Last7Days);
