using System;
using System.Collections.Generic;
using System.Linq;
using LiquidCode.Api.Contests.Requests;
using LiquidCode.Api.Contests.Responses;
using LiquidCode.Domain.Interfaces.Repositories;
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
        if (request.StartsAt >= request.EndsAt)
            return null;

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
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
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

        if (request.StartsAt.HasValue && request.EndsAt.HasValue && request.StartsAt >= request.EndsAt)
            return null;

        if (request.StartsAt.HasValue)
            contest.StartsAt = request.StartsAt.Value;

        if (request.EndsAt.HasValue)
            contest.EndsAt = request.EndsAt.Value;

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
}
