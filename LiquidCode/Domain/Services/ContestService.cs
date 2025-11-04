using System;
using System.Collections.Generic;
using System.Linq;
using LiquidCode.Api.Contests.Requests;
using LiquidCode.Api.Contests.Responses;
using LiquidCode.Domain.Interfaces.Repositories;
using LiquidCode.Domain.Interfaces.Services;
using LiquidCode.Infrastructure.Database.Entities;
using Microsoft.Extensions.Logging;

namespace LiquidCode.Domain.Services.Contests;

/// <summary>
/// Реализация бизнес-логики контестов
/// </summary>
public class ContestService : IContestService
{
    private readonly IContestRepository _contestRepository;
    private readonly IUserRepository _userRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IMissionRepository _missionRepository;
    private readonly IArticleRepository _articleRepository;
    private readonly ILogger<ContestService> _logger;

    public ContestService(
        IContestRepository contestRepository,
        IUserRepository userRepository,
        IGroupRepository groupRepository,
        IMissionRepository missionRepository,
        IArticleRepository articleRepository,
        ILogger<ContestService> logger)
    {
        _contestRepository = contestRepository;
        _userRepository = userRepository;
        _groupRepository = groupRepository;
        _missionRepository = missionRepository;
        _articleRepository = articleRepository;
        _logger = logger;
    }

    public async Task<ContestResponse?> CreateAsync(CreateContestRequest request, int creatorId, CancellationToken cancellationToken = default)
    {
        if (!TryBuildSchedule(
                request.ScheduleType,
                request.StartsAt,
                request.EndsAt,
                request.AvailableFrom,
                request.AvailableUntil,
                request.AttemptDurationMinutes,
                out var schedule,
                out var validationError))
        {
            _logger.LogWarning("Invalid contest schedule during creation: {Error}", validationError);
            return null;
        }

        var now = DateTime.UtcNow;

        var visibility = request.Visibility;
        DbGroup? group = null;
        if (visibility == ContestVisibility.GroupPrivate)
        {
            if (!request.GroupId.HasValue)
            {
                _logger.LogWarning("GroupPrivate contest requires group id");
                return null;
            }

            group = await _groupRepository.FindWithDetailsAsync(request.GroupId.Value, cancellationToken);
            if (group == null)
            {
                _logger.LogWarning("Group not found: {GroupId}", request.GroupId);
                return null;
            }

            if (!IsGroupAdmin(group, creatorId))
            {
                _logger.LogWarning("User {UserId} is not admin in group {GroupId}", creatorId, request.GroupId.Value);
                return null;
            }
        }

        var contest = new DbContest
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            ScheduleType = schedule.ScheduleType,
            Visibility = visibility,
            StartsAt = schedule.StartsAt,
            EndsAt = schedule.EndsAt,
            AvailableFrom = schedule.AvailableFrom,
            AvailableUntil = schedule.AvailableUntil,
            AttemptDurationMinutes = schedule.AttemptDurationMinutes,
            MaxAttempts = NormalizeMaxAttempts(request.MaxAttempts) ?? 1,
            AllowEarlyFinish = request.AllowEarlyFinish ?? true,
            GroupId = visibility == ContestVisibility.GroupPrivate ? request.GroupId : null,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _contestRepository.CreateAsync(contest, cancellationToken);

        await _contestRepository.UpsertMembershipAsync(
            contest.Id,
            creatorId,
            ContestMembershipRole.Organizer,
            new ContestMembershipOptions(JoinedAt: now),
            cancellationToken);

        if (visibility == ContestVisibility.GroupPrivate && group != null)
        {
            await AutoEnrollGroupAsync(contest.Id, group, now, cancellationToken);
        }

        await SyncLineupAsync(contest, request.MissionIds, request.ArticleIds, cancellationToken);
        await SyncMembersAsync(contest.Id, request.ParticipantIds, ContestMembershipRole.Participant, cancellationToken, skipUserId: creatorId);
        await SyncMembersAsync(contest.Id, request.OrganizerIds, ContestMembershipRole.Organizer, cancellationToken, skipUserId: creatorId);

        var full = await _contestRepository.FindWithDetailsAsync(contest.Id, cancellationToken);
        return full == null ? null : ContestResponse.FromEntity(full);
    }

    public async Task<ContestResponse?> UpdateAsync(int contestId, UpdateContestRequest request, int requesterId, CancellationToken cancellationToken = default)
    {
        var contest = await _contestRepository.FindWithDetailsAsync(contestId, cancellationToken);
        if (contest == null)
            return null;

        if (!IsOrganizer(contest, requesterId))
            return null;

        var newVisibility = request.Visibility ?? contest.Visibility;
        var now = DateTime.UtcNow;
        DbGroup? targetGroup = null;
        int? newGroupId = contest.GroupId;

        if (newVisibility == ContestVisibility.GroupPrivate)
        {
            var groupId = request.GroupId ?? contest.GroupId;
            if (!groupId.HasValue)
            {
                _logger.LogWarning("GroupPrivate contest requires group id on update");
                return null;
            }

            targetGroup = await _groupRepository.FindWithDetailsAsync(groupId.Value, cancellationToken);
            if (targetGroup == null)
                return null;

            if (!IsGroupAdmin(targetGroup, requesterId))
                return null;

            newGroupId = groupId;
        }
        else
        {
            newGroupId = null;
        }

        var targetScheduleType = request.ScheduleType ?? contest.ScheduleType;
        var candidateStartsAt = request.StartsAt ?? contest.StartsAt;
        var candidateEndsAt = request.EndsAt ?? contest.EndsAt;
        var candidateAvailableFrom = request.AvailableFrom ?? contest.AvailableFrom;
        var candidateAvailableUntil = request.AvailableUntil ?? contest.AvailableUntil;
        var candidateAttemptDuration = request.AttemptDurationMinutes ?? contest.AttemptDurationMinutes;

        if (!TryBuildSchedule(
                targetScheduleType,
                candidateStartsAt,
                candidateEndsAt,
                candidateAvailableFrom,
                candidateAvailableUntil,
                candidateAttemptDuration,
                out var schedule,
                out var validationError))
        {
            _logger.LogWarning("Invalid contest schedule update for contest {ContestId}: {Error}", contestId, validationError);
            return null;
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
            contest.Name = request.Name.Trim();

        if (request.Description != null)
            contest.Description = request.Description.Trim();

        contest.ScheduleType = schedule.ScheduleType;
        contest.StartsAt = schedule.StartsAt;
        contest.EndsAt = schedule.EndsAt;
        contest.AvailableFrom = schedule.AvailableFrom;
        contest.AvailableUntil = schedule.AvailableUntil;
        contest.AttemptDurationMinutes = schedule.AttemptDurationMinutes;
        contest.Visibility = newVisibility;
        contest.GroupId = newGroupId;
        if (request.MaxAttempts.HasValue)
        {
            contest.MaxAttempts = NormalizeMaxAttempts(request.MaxAttempts);
        }
        contest.AllowEarlyFinish = request.AllowEarlyFinish ?? contest.AllowEarlyFinish;
        contest.UpdatedAt = now;

        await _contestRepository.UpdateAsync(contest, cancellationToken);

        if (newVisibility == ContestVisibility.GroupPrivate && targetGroup != null)
        {
            await AutoEnrollGroupAsync(contest.Id, targetGroup, now, cancellationToken);
        }

        await SyncLineupAsync(contest, request.MissionIds, request.ArticleIds, cancellationToken);

        var updated = await _contestRepository.FindWithDetailsAsync(contest.Id, cancellationToken);
        return updated == null ? null : ContestResponse.FromEntity(updated);
    }

    public async Task<bool> DeleteAsync(int contestId, int requesterId, CancellationToken cancellationToken = default)
    {
        var contest = await _contestRepository.FindWithDetailsAsync(contestId, cancellationToken);
        if (contest == null)
            return false;

        if (!IsOrganizer(contest, requesterId))
            return false;

        await _contestRepository.SoftDeleteAsync(contest, cancellationToken);
        return true;
    }

    public async Task<ContestResponse?> GetAsync(int contestId, CancellationToken cancellationToken = default)
    {
        var contest = await _contestRepository.FindWithDetailsAsync(contestId, cancellationToken);
        return contest == null ? null : ContestResponse.FromEntity(contest);
    }

    public async Task<ContestsPageResponse?> GetUpcomingAsync(int pageSize, int pageNumber, CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0 || pageNumber < 0)
            return null;

        var (contests, hasNext) = await _contestRepository.GetUpcomingAsync(pageSize, pageNumber, DateTime.UtcNow, cancellationToken);
        var filtered = contests
            .Where(c => c.Visibility == ContestVisibility.Public)
            .Select(ContestResponse.FromEntity)
            .ToList();

        return new ContestsPageResponse(hasNext, filtered);
    }

    public async Task<ContestsPageResponse?> GetForGroupAsync(int groupId, int pageSize, int pageNumber, CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0 || pageNumber < 0)
            return null;

        var (contests, hasNext) = await _contestRepository.GetByGroupAsync(groupId, pageSize, pageNumber, cancellationToken);
        var responses = contests.Select(ContestResponse.FromEntity).ToList();
        return new ContestsPageResponse(hasNext, responses);
    }

    public async Task<bool> UpsertMemberAsync(int contestId, int requesterId, int targetUserId, ContestMembershipRole role, CancellationToken cancellationToken = default)
    {
        var contest = await _contestRepository.FindWithDetailsAsync(contestId, cancellationToken);
        if (contest == null)
            return false;

        if (!IsOrganizer(contest, requesterId))
            return false;

        if (await _userRepository.FindByIdAsync(targetUserId, cancellationToken) == null)
        {
            _logger.LogWarning("User {UserId} not found while adding to contest {ContestId}", targetUserId, contestId);
            return false;
        }

        var now = DateTime.UtcNow;
        await _contestRepository.UpsertMembershipAsync(
            contestId,
            targetUserId,
            role,
            new ContestMembershipOptions(JoinedAt: now),
            cancellationToken);

        return true;
    }

    public async Task<bool> RemoveMemberAsync(int contestId, int requesterId, int targetUserId, CancellationToken cancellationToken = default)
    {
        var contest = await _contestRepository.FindWithDetailsAsync(contestId, cancellationToken);
        if (contest == null)
            return false;

        if (!IsOrganizer(contest, requesterId))
            return false;

        var membership = contest.Memberships.FirstOrDefault(m => m.UserId == targetUserId);
        if (membership == null)
            return false;

        if (membership.Role.HasFlag(ContestMembershipRole.Organizer) && targetUserId == requesterId)
        {
            if (contest.Memberships.Count(m => m.Role.HasFlag(ContestMembershipRole.Organizer)) <= 1)
            {
                _logger.LogWarning("Cannot remove the last organizer from contest {ContestId}", contestId);
                return false;
            }
        }

        await _contestRepository.RemoveMembershipAsync(contestId, targetUserId, cancellationToken);
        return true;
    }

    public async Task<ContestAttemptResponse?> StartAttemptAsync(int contestId, int userId, CancellationToken cancellationToken = default)
    {
        var contest = await _contestRepository.FindWithDetailsAsync(contestId, cancellationToken);
        if (contest == null || contest.IsDeleted)
            return null;

        var now = DateTime.UtcNow;
        var membership = contest.Memberships.FirstOrDefault(m => m.UserId == userId);

        if (membership == null)
        {
            if (contest.Visibility != ContestVisibility.Public)
            {
                _logger.LogWarning("User {UserId} is not allowed to join contest {ContestId}", userId, contestId);
                return null;
            }

            await _contestRepository.UpsertMembershipAsync(
                contestId,
                userId,
                ContestMembershipRole.Participant,
                new ContestMembershipOptions(IsAutoJoined: true, JoinedAt: now),
                cancellationToken);

            contest = await _contestRepository.FindWithDetailsAsync(contestId, cancellationToken);
            membership = contest?.Memberships.FirstOrDefault(m => m.UserId == userId);
            if (membership == null)
                return null;
        }

        if (contest == null)
            return null;

        var isOrganizer = membership.Role.HasFlag(ContestMembershipRole.Organizer);

        var missions = contest.Missions;
        if (missions == null || missions.Count == 0)
        {
            _logger.LogWarning("Contest {ContestId} has no missions configured", contestId);
            return null;
        }

        membership = contest.Memberships.First(m => m.UserId == userId);

        var activeAttempt = membership.ActiveAttempt;
        if (activeAttempt != null && activeAttempt.Status == ContestAttemptStatus.Active)
        {
            if (activeAttempt.ExpiresAt.HasValue && now > activeAttempt.ExpiresAt.Value)
            {
                activeAttempt.Status = ContestAttemptStatus.Expired;
                activeAttempt.FinishedAt = activeAttempt.ExpiresAt;
                activeAttempt.FinishedBy = ContestAttemptFinishReason.Timer;
                membership.ActiveAttemptId = null;
                membership.ActiveAttempt = null;
                await _contestRepository.UpdateAttemptAsync(activeAttempt, cancellationToken);
                await _contestRepository.SaveChangesAsync(cancellationToken);
            }
            else
            {
                return ToAttemptResponse(activeAttempt, contest.ScheduleType);
            }
        }

        if (contest.MaxAttempts.HasValue)
        {
            var attemptsCount = membership.Attempts.Count;
            if (attemptsCount >= contest.MaxAttempts.Value)
            {
                _logger.LogWarning("User {UserId} reached max attempts for contest {ContestId}", userId, contestId);
                return null;
            }
        }

        if (!IsContestAccessibleForStart(contest, now, isOrganizer))
            return null;

        var expireAt = CalculateAttemptExpiration(contest, now);
        if (expireAt != null && expireAt <= now)
            return null;

        var attemptIndex = membership.Attempts.Count + 1;
        var attempt = new DbContestAttempt
        {
            ContestId = contest.Id,
            UserId = userId,
            AttemptIndex = attemptIndex,
            Status = ContestAttemptStatus.Active,
            StartedAt = now,
            ExpiresAt = expireAt,
            Membership = membership
        };

        await _contestRepository.AddAttemptAsync(attempt, cancellationToken);

        membership.ActiveAttemptId = attempt.Id;
        membership.ActiveAttempt = attempt;
        membership.LastAttemptStartedAt = now;
        membership.Attempts.Add(attempt);

        await _contestRepository.SaveChangesAsync(cancellationToken);

        return ToAttemptResponse(attempt, contest.ScheduleType);
    }

    private async Task AutoEnrollGroupAsync(int contestId, DbGroup group, DateTime joinedAt, CancellationToken cancellationToken)
    {
        foreach (var member in group.Memberships)
        {
            var role = member.Role.HasFlag(GroupMembershipRole.Administrator)
                ? ContestMembershipRole.Organizer
                : ContestMembershipRole.Participant;

            await _contestRepository.UpsertMembershipAsync(
                contestId,
                member.UserId,
                role,
                new ContestMembershipOptions(IsAutoJoined: true, JoinedAt: member.JoinedAt <= DateTime.MinValue ? joinedAt : member.JoinedAt),
                cancellationToken);
        }
    }

    private async Task SyncLineupAsync(DbContest contest, IEnumerable<int>? missionIds, IEnumerable<int>? articleIds, CancellationToken cancellationToken)
    {
        if (missionIds != null)
        {
            var filteredMissions = await FilterExistingMissionIdsAsync(missionIds, cancellationToken);
            await _contestRepository.SyncMissionsAsync(contest, filteredMissions, cancellationToken);
        }

        if (articleIds != null)
        {
            var filteredArticles = await FilterExistingArticleIdsAsync(articleIds, cancellationToken);
            await _contestRepository.SyncArticlesAsync(contest, filteredArticles, cancellationToken);
        }
    }

    private async Task<IEnumerable<int>> FilterExistingMissionIdsAsync(IEnumerable<int> missionIds, CancellationToken cancellationToken)
    {
        var result = new List<int>();
        foreach (var missionId in missionIds.Distinct())
        {
            if (await _missionRepository.FindByIdAsync(missionId, cancellationToken) != null)
            {
                result.Add(missionId);
            }
            else
            {
                _logger.LogWarning("Mission {MissionId} not found while syncing contest lineup", missionId);
            }
        }

        return result;
    }

    private async Task<IEnumerable<int>> FilterExistingArticleIdsAsync(IEnumerable<int> articleIds, CancellationToken cancellationToken)
    {
        var result = new List<int>();
        foreach (var articleId in articleIds.Distinct())
        {
            if (await _articleRepository.FindByIdAsync(articleId, cancellationToken) != null)
            {
                result.Add(articleId);
            }
            else
            {
                _logger.LogWarning("Article {ArticleId} not found while syncing contest lineup", articleId);
            }
        }

        return result;
    }

    private async Task SyncMembersAsync(int contestId, IEnumerable<int>? userIds, ContestMembershipRole role, CancellationToken cancellationToken, int? skipUserId = null)
    {
        if (userIds == null)
            return;

        var now = DateTime.UtcNow;
        foreach (var userId in userIds.Where(id => id != skipUserId).Distinct())
        {
            if (await _userRepository.FindByIdAsync(userId, cancellationToken) != null)
            {
                await _contestRepository.UpsertMembershipAsync(
                    contestId,
                    userId,
                    role,
                    new ContestMembershipOptions(JoinedAt: now),
                    cancellationToken);
            }
            else
            {
                _logger.LogWarning("User {UserId} not found while syncing contest members", userId);
            }
        }
    }

    private static bool IsOrganizer(DbContest contest, int userId)
    {
        var membership = contest.Memberships.FirstOrDefault(m => m.UserId == userId);
        return membership != null && membership.Role.HasFlag(ContestMembershipRole.Organizer);
    }

    private static bool IsGroupAdmin(DbGroup group, int userId)
    {
        var membership = group.Memberships.FirstOrDefault(m => m.UserId == userId);
        return membership != null && membership.Role.HasFlag(GroupMembershipRole.Administrator);
    }

    private static bool TryBuildSchedule(
        ContestScheduleType scheduleType,
        DateTime? startsAt,
        DateTime? endsAt,
        DateTime? availableFrom,
        DateTime? availableUntil,
        int? attemptDurationMinutes,
        out ContestScheduleData schedule,
        out string? error)
    {
        schedule = default!;
        error = null;

        switch (scheduleType)
        {
            case ContestScheduleType.AlwaysOpen:
                if (!attemptDurationMinutes.HasValue || attemptDurationMinutes.Value <= 0)
                {
                    error = "AlwaysOpen contests require positive attempt duration.";
                    return false;
                }

                schedule = new ContestScheduleData(scheduleType, null, null, null, null, attemptDurationMinutes.Value);
                return true;

            case ContestScheduleType.FixedWindow:
                if (!startsAt.HasValue || !endsAt.HasValue)
                {
                    error = "FixedWindow contests require start and end time.";
                    return false;
                }

                if (startsAt.Value >= endsAt.Value)
                {
                    error = "Contest start must be before end.";
                    return false;
                }

                schedule = new ContestScheduleData(scheduleType, startsAt.Value, endsAt.Value, null, null, attemptDurationMinutes);
                return true;

            case ContestScheduleType.RollingWindow:
                if (!availableFrom.HasValue || !availableUntil.HasValue)
                {
                    error = "RollingWindow contests require availability window.";
                    return false;
                }

                if (!attemptDurationMinutes.HasValue || attemptDurationMinutes.Value <= 0)
                {
                    error = "RollingWindow contests require positive attempt duration.";
                    return false;
                }

                if (availableFrom.Value >= availableUntil.Value)
                {
                    error = "Availability window start must be before end.";
                    return false;
                }

                var totalWindowMinutes = (int)(availableUntil.Value - availableFrom.Value).TotalMinutes;
                if (attemptDurationMinutes.Value > totalWindowMinutes)
                {
                    error = "Attempt duration cannot exceed availability window.";
                    return false;
                }

                schedule = new ContestScheduleData(scheduleType, null, null, availableFrom.Value, availableUntil.Value, attemptDurationMinutes.Value);
                return true;

            default:
                error = $"Unsupported schedule type {scheduleType}";
                return false;
        }
    }

    private static int? NormalizeMaxAttempts(int? maxAttempts) =>
        maxAttempts.HasValue && maxAttempts.Value > 0 ? maxAttempts : null;

    private static bool IsContestAccessibleForStart(DbContest contest, DateTime now, bool isOrganizer)
    {
        switch (contest.ScheduleType)
        {
            case ContestScheduleType.AlwaysOpen:
                return true;
            case ContestScheduleType.FixedWindow:
                if (!contest.StartsAt.HasValue || !contest.EndsAt.HasValue)
                    return false;
                return isOrganizer || (now >= contest.StartsAt.Value && now <= contest.EndsAt.Value);
            case ContestScheduleType.RollingWindow:
                if (!contest.AvailableFrom.HasValue || !contest.AvailableUntil.HasValue)
                    return false;
                return isOrganizer || (now >= contest.AvailableFrom.Value && now <= contest.AvailableUntil.Value);
            default:
                return false;
        }
    }

    private static DateTime? CalculateAttemptExpiration(DbContest contest, DateTime start)
    {
        switch (contest.ScheduleType)
        {
            case ContestScheduleType.AlwaysOpen:
                return contest.AttemptDurationMinutes.HasValue
                    ? start.AddMinutes(contest.AttemptDurationMinutes.Value)
                    : null;
            case ContestScheduleType.FixedWindow:
                return contest.EndsAt;
            case ContestScheduleType.RollingWindow:
                if (!contest.AttemptDurationMinutes.HasValue || !contest.AvailableUntil.HasValue)
                    return null;

                var desired = start.AddMinutes(contest.AttemptDurationMinutes.Value);
                return desired <= contest.AvailableUntil.Value ? desired : contest.AvailableUntil.Value;
            default:
                return null;
        }
    }

    private static ContestAttemptResponse ToAttemptResponse(DbContestAttempt attempt, ContestScheduleType scheduleType) =>
        new(
            attempt.Id,
            attempt.ContestId,
            attempt.UserId,
            attempt.AttemptIndex,
            attempt.Status,
            scheduleType,
            attempt.StartedAt,
            attempt.ExpiresAt,
            attempt.FinishedAt,
            attempt.TotalScore,
            attempt.SolvedCount);

    private sealed record ContestScheduleData(
        ContestScheduleType ScheduleType,
        DateTime? StartsAt,
        DateTime? EndsAt,
        DateTime? AvailableFrom,
        DateTime? AvailableUntil,
        int? AttemptDurationMinutes
    );
}
