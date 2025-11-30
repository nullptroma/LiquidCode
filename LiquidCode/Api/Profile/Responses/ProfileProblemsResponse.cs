using System.Collections.Generic;

namespace LiquidCode.Api.Profile.Responses;

/// <summary>
/// Ответ со статистикой по задачам профиля
/// </summary>
/// <param name="Summary">Краткая сводка по задачам</param>
/// <param name="Recent">Последние активности пользователя по задачам</param>
/// <param name="Authored">Задачи, созданные пользователем</param>
public record ProfileProblemsResponse(
    ProfileProblemsSummaryResponse Summary,
    ProfilePagedResponse<ProfileRecentMissionResponse> Recent,
    ProfilePagedResponse<ProfileAuthoredMissionResponse> Authored);

public record ProfileProblemsSummaryResponse(
    ProfileProblemCounterResponse Total,
    IReadOnlyList<ProfileProblemCounterResponse> Buckets);

public record ProfileProblemCounterResponse(string Key, string Label, int Solved, int Total);

public record ProfileRecentMissionResponse(
    int MissionId,
    string MissionName,
    string DifficultyLabel,
    int DifficultyValue,
    bool IsAccepted,
    string Status,
    DateTime SubmittedAt,
    int? TimeLimitMilliseconds,
    int? MemoryLimitBytes);
