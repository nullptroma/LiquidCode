using System;
using System.Collections.Generic;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Domain.Interfaces.Repositories;

/// <summary>
/// Репозиторий для управления контестами
/// </summary>
public interface IContestRepository : IRepository<DbContest>
{
    Task<DbContest?> FindWithDetailsAsync(int id, CancellationToken cancellationToken = default);

    Task<(IEnumerable<DbContest> Items, bool HasNextPage)> GetUpcomingAsync(
        int pageSize,
        int pageNumber,
        DateTime? from,
        CancellationToken cancellationToken = default);

    Task<(IEnumerable<DbContest> Items, bool HasNextPage)> GetByGroupAsync(
        int groupId,
        int pageSize,
        int pageNumber,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DbContest>> GetOrganizedByUserAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<(IEnumerable<DbContest> Items, bool HasNextPage)> GetParticipatingAsync(
        int userId,
        int pageSize,
        int pageNumber,
        CancellationToken cancellationToken = default);

    Task<(IEnumerable<DbContestMembership> Items, bool HasNextPage)> GetMembersPageAsync(
        int contestId,
        int pageSize,
        int pageNumber,
        CancellationToken cancellationToken = default);

    Task SyncMissionsAsync(DbContest contest, IEnumerable<int> missionIds, CancellationToken cancellationToken = default);
    Task SyncArticlesAsync(DbContest contest, IEnumerable<int> articleIds, CancellationToken cancellationToken = default);

    Task UpsertMembershipAsync(int contestId, int userId, ContestMembershipRole role, ContestMembershipOptions? options, CancellationToken cancellationToken = default);
    Task RemoveMembershipAsync(int contestId, int userId, CancellationToken cancellationToken = default);
    Task<DbContestMembership?> GetMembershipAsync(int contestId, int userId, CancellationToken cancellationToken = default);
    Task<DbContestAttempt?> FindActiveAttemptAsync(int contestId, int userId, CancellationToken cancellationToken = default);
    Task AddAttemptAsync(DbContestAttempt attempt, CancellationToken cancellationToken = default);
    Task UpdateAttemptAsync(DbContestAttempt attempt, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DbContestAttempt>> GetUserAttemptsAsync(int contestId, int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DbContestAttemptMissionResult>> GetAttemptResultsAsync(int attemptId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DbContestAttempt>> GetAttemptsByUserAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DbContest>> GetUpcomingRegisteredAsync(int userId, DateTime asOf, CancellationToken cancellationToken = default);
}

public record ContestMembershipOptions(
    bool IsAutoJoined = false,
    int? InvitationId = null,
    DateTime? JoinedAt = null
);
