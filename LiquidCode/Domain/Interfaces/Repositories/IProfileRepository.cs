using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LiquidCode.Domain.Interfaces.Repositories;

public interface IProfileRepository
{
    Task<ProfileUserPlacement> GetUserPlacementAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MissionSolvedProjection>> GetSolvedMissionsAsync(int userId, CancellationToken cancellationToken = default);
    Task<int> CountSolvedMissionsSinceAsync(int userId, DateTime fromUtc, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MissionDifficultyCount>> GetMissionDifficultyTotalsAsync(int easyThreshold, int mediumThreshold, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TagSolvedProjection>> GetCompetencyStatsAsync(IReadOnlyCollection<int> solvedMissionIds, int limit, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TagTotalProjection>> GetTagTotalsAsync(IReadOnlyCollection<int> tagIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SubmissionProjection>> GetRecentSubmissionsAsync(int userId, int limit, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuthoredMissionProjection>> GetAuthoredMissionsAsync(int userId, int limit, CancellationToken cancellationToken = default);
    Task<ContestActivityMetrics> GetContestActivityAsync(int userId, DateTime fromUtc, CancellationToken cancellationToken = default);
    Task<CreationActivityMetrics> GetCreationActivityAsync(int userId, DateTime fromUtc, CancellationToken cancellationToken = default);
}

public record ProfileUserPlacement(int TotalUsers, int AcceptedCount, int HigherAcceptedUsersCount);

public record MissionSolvedProjection(int MissionId, int Difficulty);

public record MissionDifficultyCount(int BucketKey, int Count);

public record TagSolvedProjection(int TagId, string TagName, int SolvedCount);

public record TagTotalProjection(int TagId, int TotalCount);

public record SubmissionProjection(
    int MissionId,
    string MissionName,
    int Difficulty,
    string Status,
    DateTime CreatedAt,
    int? TimeLimitMilliseconds,
    int? MemoryLimitBytes);

public record AuthoredMissionProjection(
    int MissionId,
    string MissionName,
    int Difficulty,
    DateTime CreatedAt,
    int? TimeLimitMilliseconds,
    int? MemoryLimitBytes);

public record ContestActivityMetrics(int TotalAttempts, int AttemptsLastPeriod);

public record CreationActivityMetrics(
    int MissionsTotal,
    int MissionsLastPeriod,
    int ArticlesTotal,
    int ArticlesLastPeriod,
    int ContestsTotal,
    int ContestsLastPeriod);
