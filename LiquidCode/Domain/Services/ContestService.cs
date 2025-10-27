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

        var creator = await _userRepository.FindByIdAsync(creatorId, cancellationToken);
        if (creator == null)
            return null;

        DbGroup? group = null;
        if (request.GroupId.HasValue)
        {
            group = await _groupRepository.FindWithDetailsAsync(request.GroupId.Value, cancellationToken);
            if (group == null)
            {
                _logger.LogWarning("Group not found: {GroupId}", request.GroupId);
                return null;
            }

            var membership = group.Memberships.FirstOrDefault(m => m.UserId == creatorId);
            if (membership == null || !membership.Role.HasFlag(GroupMembershipRole.Administrator))
            {
                _logger.LogWarning("User {UserId} is not admin in group {GroupId}", creatorId, group.Id);
                return null;
            }
        }

        var contest = new DbContest
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            ScheduleType = schedule.ScheduleType,
            StartsAt = schedule.StartsAt,
            EndsAt = schedule.EndsAt,
            AvailableFrom = schedule.AvailableFrom,
            AvailableUntil = schedule.AvailableUntil,
            AttemptDurationMinutes = schedule.AttemptDurationMinutes,
            GroupId = request.GroupId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _contestRepository.CreateAsync(contest, cancellationToken);
        await _contestRepository.UpsertMembershipAsync(contest.Id, creatorId, ContestMembershipRole.Organizer, cancellationToken);

        await SyncLineupAsync(contest, request.MissionIds, request.ArticleIds, cancellationToken);
        await SyncMembersAsync(contest.Id, request.ParticipantIds, ContestMembershipRole.Participant, cancellationToken);
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

        if (!string.IsNullOrWhiteSpace(request.Name))
            contest.Name = request.Name.Trim();

        if (request.Description != null)
            contest.Description = request.Description.Trim();

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

        var previousScheduleType = contest.ScheduleType;
        var previousAvailableFrom = contest.AvailableFrom;
        var previousAvailableUntil = contest.AvailableUntil;
        var previousAttemptDuration = contest.AttemptDurationMinutes;

        contest.ScheduleType = schedule.ScheduleType;
        contest.StartsAt = schedule.StartsAt;
        contest.EndsAt = schedule.EndsAt;
        contest.AvailableFrom = schedule.AvailableFrom;
        contest.AvailableUntil = schedule.AvailableUntil;
        contest.AttemptDurationMinutes = schedule.AttemptDurationMinutes;

        if (previousScheduleType != schedule.ScheduleType ||
            (schedule.ScheduleType == ContestScheduleType.FlexibleWindow &&
             (previousAvailableFrom != schedule.AvailableFrom ||
              previousAvailableUntil != schedule.AvailableUntil ||
              previousAttemptDuration != schedule.AttemptDurationMinutes)))
        {
            ResetFlexibleAttempts(contest);
        }

        contest.UpdatedAt = DateTime.UtcNow;

        await _contestRepository.UpdateAsync(contest, cancellationToken);

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
        return new ContestsPageResponse(hasNext, contests.Select(ContestResponse.FromEntity));
    }

    public async Task<ContestsPageResponse?> GetForGroupAsync(int groupId, int pageSize, int pageNumber, CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0 || pageNumber < 0)
            return null;

        var (contests, hasNext) = await _contestRepository.GetByGroupAsync(groupId, pageSize, pageNumber, cancellationToken);
        return new ContestsPageResponse(hasNext, contests.Select(ContestResponse.FromEntity));
    }

    public async Task<bool> UpsertMemberAsync(int contestId, int requesterId, int targetUserId, ContestMembershipRole role, CancellationToken cancellationToken = default)
    {
        var contest = await _contestRepository.FindWithDetailsAsync(contestId, cancellationToken);
        if (contest == null)
            return false;

        if (!IsOrganizer(contest, requesterId))
            return false;

        await _contestRepository.UpsertMembershipAsync(contestId, targetUserId, role, cancellationToken);
        return true;
    }

    public async Task<bool> RemoveMemberAsync(int contestId, int requesterId, int targetUserId, CancellationToken cancellationToken = default)
    {
        var contest = await _contestRepository.FindWithDetailsAsync(contestId, cancellationToken);
        if (contest == null)
            return false;

        if (!IsOrganizer(contest, requesterId))
            return false;

        await _contestRepository.RemoveMembershipAsync(contestId, targetUserId, cancellationToken);
        return true;
    }

    public async Task<ContestAttemptResponse?> StartAttemptAsync(int contestId, int userId, CancellationToken cancellationToken = default)
    {
        var contest = await _contestRepository.FindWithDetailsAsync(contestId, cancellationToken);
        if (contest == null || contest.IsDeleted)
            return null;

        if (contest.ScheduleType != ContestScheduleType.FlexibleWindow)
        {
            _logger.LogWarning("Attempt start requested for contest {ContestId} with schedule type {ScheduleType}", contestId, contest.ScheduleType);
            return null;
        }

        if (!contest.AvailableFrom.HasValue || !contest.AvailableUntil.HasValue || !contest.AttemptDurationMinutes.HasValue)
        {
            _logger.LogWarning("Contest {ContestId} has inconsistent flexible schedule configuration", contestId);
            return null;
        }

        var membership = contest.Memberships.FirstOrDefault(m => m.UserId == userId);
        if (membership == null)
        {
            _logger.LogWarning("User {UserId} is not enrolled in contest {ContestId}", userId, contestId);
            return null;
        }

        var now = DateTime.UtcNow;
        var isOrganizer = membership.Role.HasFlag(ContestMembershipRole.Organizer);

        if (!isOrganizer && (now < contest.AvailableFrom.Value || now > contest.AvailableUntil.Value))
        {
            _logger.LogWarning("Contest {ContestId} is not available for starting attempt by user {UserId}", contestId, userId);
            return null;
        }

        if (membership.ActiveAttemptStartedAt.HasValue &&
            membership.ActiveAttemptExpiresAt.HasValue &&
            now <= membership.ActiveAttemptExpiresAt.Value)
        {
            return new ContestAttemptResponse(
                contest.Id,
                userId,
                contest.ScheduleType,
                membership.ActiveAttemptStartedAt.Value,
                membership.ActiveAttemptExpiresAt.Value,
                membership.AttemptCount);
        }

        var expireAt = now.AddMinutes(contest.AttemptDurationMinutes.Value);
        if (contest.AvailableUntil.Value < expireAt)
        {
            expireAt = contest.AvailableUntil.Value;
        }

        if (expireAt <= now)
        {
            _logger.LogWarning("Calculated attempt window is invalid for contest {ContestId} and user {UserId}", contestId, userId);
            return null;
        }

        membership.ActiveAttemptStartedAt = now;
        membership.ActiveAttemptExpiresAt = expireAt;
        membership.AttemptCount += 1;
        membership.UpdatedAt = DateTime.UtcNow;

        await _contestRepository.SaveChangesAsync(cancellationToken);

        return new ContestAttemptResponse(
            contest.Id,
            userId,
            contest.ScheduleType,
            membership.ActiveAttemptStartedAt.Value,
            membership.ActiveAttemptExpiresAt.Value,
            membership.AttemptCount);
    }

    private static void ResetFlexibleAttempts(DbContest contest)
    {
        foreach (var membership in contest.Memberships)
        {
            membership.ActiveAttemptStartedAt = null;
            membership.ActiveAttemptExpiresAt = null;
            membership.UpdatedAt = DateTime.UtcNow;
        }
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
            case ContestScheduleType.FixedWindow when !startsAt.HasValue || !endsAt.HasValue:
                error = "Для фиксированного контеста необходимо указать время начала и окончания.";
                return false;
            case ContestScheduleType.FixedWindow when startsAt!.Value >= endsAt!.Value:
                error = "Время начала должно быть раньше времени окончания.";
                return false;
            case ContestScheduleType.FixedWindow:
                schedule = new ContestScheduleData(
                    scheduleType,
                    startsAt.Value,
                    endsAt.Value,
                    null,
                    null,
                    null);
                return true;

            case ContestScheduleType.FlexibleWindow when !availableFrom.HasValue || !availableUntil.HasValue:
                error = "Для гибкого контеста необходимо указать окно доступности.";
                return false;
            case ContestScheduleType.FlexibleWindow when !attemptDurationMinutes.HasValue:
                error = "Не указана длительность попытки.";
                return false;
            case ContestScheduleType.FlexibleWindow when availableFrom!.Value >= availableUntil!.Value:
                error = "Начало окна должно быть раньше окончания.";
                return false;
            case ContestScheduleType.FlexibleWindow when attemptDurationMinutes!.Value <= 0:
                error = "Длительность попытки должна быть положительной.";
                return false;
            case ContestScheduleType.FlexibleWindow:
                var totalWindowMinutes = (int)(availableUntil.Value - availableFrom.Value).TotalMinutes;
                if (totalWindowMinutes <= 0)
                {
                    error = "Окно доступности слишком короткое.";
                    return false;
                }

                if (attemptDurationMinutes.Value > totalWindowMinutes)
                {
                    error = "Длительность попытки не может превышать окно доступности.";
                    return false;
                }

                schedule = new ContestScheduleData(
                    scheduleType,
                    null,
                    null,
                    availableFrom.Value,
                    availableUntil.Value,
                    attemptDurationMinutes.Value);
                return true;

            default:
                error = $"Неизвестный тип расписания: {scheduleType}";
                return false;
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

        var distinct = userIds
            .Where(id => id != skipUserId)
            .Distinct()
            .ToList();

        foreach (var userId in distinct)
        {
            await _contestRepository.UpsertMembershipAsync(contestId, userId, role, cancellationToken);
        }
    }

    private static bool IsOrganizer(DbContest contest, int userId)
    {
        var membership = contest.Memberships.FirstOrDefault(m => m.UserId == userId);
        return membership != null && membership.Role.HasFlag(ContestMembershipRole.Organizer);
    }

    private sealed record ContestScheduleData(
        ContestScheduleType ScheduleType,
        DateTime? StartsAt,
        DateTime? EndsAt,
        DateTime? AvailableFrom,
        DateTime? AvailableUntil,
        int? AttemptDurationMinutes
    );
}
