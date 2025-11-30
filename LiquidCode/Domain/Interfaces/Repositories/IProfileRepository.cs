using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Domain.Interfaces.Repositories;

public interface IProfileRepository
{
    Task<ProfileUserPlacement> GetUserPlacementAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MissionSolvedProjection>> GetSolvedMissionsAsync(int userId, CancellationToken cancellationToken = default);
    Task<int> CountSolvedMissionsSinceAsync(int userId, DateTime fromUtc, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MissionDifficultyCount>> GetMissionDifficultyTotalsAsync(int easyThreshold, int mediumThreshold, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TagSolvedProjection>> GetCompetencyStatsAsync(IReadOnlyCollection<int> solvedMissionIds, int limit, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TagTotalProjection>> GetTagTotalsAsync(IReadOnlyCollection<int> tagIds, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<ProfileRecentMissionProjection> Items, bool HasNextPage)> GetRecentMissionActivitiesAsync(int userId, int pageSize, int pageNumber, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<AuthoredMissionProjection> Items, bool HasNextPage)> GetAuthoredMissionsPageAsync(int userId, int pageSize, int pageNumber, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<ProfileArticleProjection> Items, bool HasNextPage)> GetArticlesPageAsync(int userId, int pageSize, int pageNumber, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<ProfileContestProjection> Items, bool HasNextPage)> GetUserContestsPageAsync(int userId, ProfileContestFilter filter, int pageSize, int pageNumber, CancellationToken cancellationToken = default);
    Task<ContestActivityMetrics> GetContestActivityAsync(int userId, DateTime fromUtc, CancellationToken cancellationToken = default);
    Task<CreationActivityMetrics> GetCreationActivityAsync(int userId, DateTime fromUtc, CancellationToken cancellationToken = default);
}

public record ProfileUserPlacement(int TotalUsers, int AcceptedCount, int HigherAcceptedUsersCount);

public record MissionSolvedProjection(int MissionId, int Difficulty);

public record MissionDifficultyCount(int BucketKey, int Count);

public record TagSolvedProjection(int TagId, string TagName, int SolvedCount);

public record TagTotalProjection(int TagId, int TotalCount);

public record ProfileSubmissionProjection(
    int SubmissionId,
    string Status,
    DateTime CreatedAt,
    bool IsAccepted,
    int? TimeLimitMilliseconds,
    int? MemoryLimitBytes);

public record ProfileRecentMissionProjection(
    int MissionId,
    string MissionName,
    int Difficulty,
    ProfileSubmissionProjection? LatestAccepted,
    ProfileSubmissionProjection LatestSubmission);

public record AuthoredMissionProjection(
    int MissionId,
    string MissionName,
    int Difficulty,
    DateTime CreatedAt,
    int? TimeLimitMilliseconds,
    int? MemoryLimitBytes);

public record ProfileArticleProjection(
    int ArticleId,
    string Title,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record ProfileContestProjection(
    int ContestId,
    string Name,
    ContestScheduleType ScheduleType,
    ContestVisibility Visibility,
    DateTime? StartsAt,
    DateTime? EndsAt,
    int? AttemptDurationMinutes,
    ContestMembershipRole Role);

public record ContestActivityMetrics(int TotalAttempts, int AttemptsLastPeriod);

public record CreationActivityMetrics(
    int MissionsTotal,
    int MissionsLastPeriod,
    int ArticlesTotal,
    int ArticlesLastPeriod,
    int ContestsTotal,
    int ContestsLastPeriod);

public enum ProfileContestFilter
{
    Upcoming,
    Past,
    Organized
}
