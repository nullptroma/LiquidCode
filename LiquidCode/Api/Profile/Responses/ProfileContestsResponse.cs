using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Profile.Responses;

/// <summary>
/// Ответ со списками контестов пользователя
/// </summary>
/// <param name="Upcoming">Предстоящие контесты</param>
/// <param name="Past">История контестов</param>
/// <param name="Mine">Контесты, где пользователь организатор</param>
public record ProfileContestsResponse(
    ProfilePagedResponse<ProfileContestResponse> Upcoming,
    ProfilePagedResponse<ProfileContestResponse> Past,
    ProfilePagedResponse<ProfileContestResponse> Mine);

public record ProfileContestResponse(
    int ContestId,
    string Name,
    ContestScheduleType ScheduleType,
    ContestVisibility Visibility,
    DateTime? StartsAt,
    DateTime? EndsAt,
    int? AttemptDurationMinutes,
    ContestMembershipRole Role);
