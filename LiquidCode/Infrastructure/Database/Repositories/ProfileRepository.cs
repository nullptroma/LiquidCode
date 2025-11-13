using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiquidCode.Domain.Interfaces.Repositories;
using LiquidCode.Infrastructure.Database;
using LiquidCode.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Repositories;

public class ProfileRepository : IProfileRepository
{
    private const string AcceptedStatusPrefix = "Accepted";
    private readonly LiquidDbContext _dbContext;

    public ProfileRepository(LiquidDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProfileUserPlacement> GetUserPlacementAsync(int userId, CancellationToken cancellationToken = default)
    {
        var totalUsers = await _dbContext.Users
            .AsNoTracking()
            .CountAsync(u => !u.IsDeleted, cancellationToken);

        var acceptedPerUserQuery = _dbContext.UserSubmits
            .AsNoTracking()
            .Where(s => !s.IsDeleted &&
                        !s.Solution.Mission.IsDeleted &&
                        s.Solution.Status.StartsWith(AcceptedStatusPrefix))
            .GroupBy(s => s.User.Id)
            .Select(g => new { UserId = g.Key, Count = g.Count() });

        var userAccepted = await acceptedPerUserQuery
            .Where(x => x.UserId == userId)
            .Select(x => (int?)x.Count)
            .FirstOrDefaultAsync(cancellationToken) ?? 0;

        var higher = totalUsers == 0 || userAccepted == 0
            ? 0
            : await acceptedPerUserQuery
                .Where(x => x.Count > userAccepted)
                .CountAsync(cancellationToken);

        return new ProfileUserPlacement(totalUsers, userAccepted, higher);
    }

    public async Task<IReadOnlyList<MissionSolvedProjection>> GetSolvedMissionsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var items = await _dbContext.UserSubmits
            .AsNoTracking()
            .Where(s => !s.IsDeleted &&
                        s.User.Id == userId &&
                        !s.Solution.Mission.IsDeleted &&
                        s.Solution.Status.StartsWith(AcceptedStatusPrefix))
            .Select(s => new
            {
                s.Solution.Mission.Id,
                s.Solution.Mission.Difficulty
            })
            .Distinct()
            .ToListAsync(cancellationToken);

        return items
            .Select(x => new MissionSolvedProjection(x.Id, x.Difficulty))
            .ToList();
    }

    public async Task<int> CountSolvedMissionsSinceAsync(int userId, DateTime fromUtc, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserSubmits
            .AsNoTracking()
            .Where(s => !s.IsDeleted &&
                        s.User.Id == userId &&
                        s.CreatedAt >= fromUtc &&
                        !s.Solution.Mission.IsDeleted &&
                        s.Solution.Status.StartsWith(AcceptedStatusPrefix))
            .Select(s => s.Solution.Mission.Id)
            .Distinct()
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MissionDifficultyCount>> GetMissionDifficultyTotalsAsync(int easyThreshold, int mediumThreshold, CancellationToken cancellationToken = default)
    {
        var results = await _dbContext.Missions
            .AsNoTracking()
            .Where(m => !m.IsDeleted)
            .Select(m => m.Difficulty <= easyThreshold
                ? 0
                : m.Difficulty <= mediumThreshold
                    ? 1
                    : 2)
            .GroupBy(bucket => bucket)
            .Select(g => new MissionDifficultyCount(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        return results;
    }

    public async Task<IReadOnlyList<TagSolvedProjection>> GetCompetencyStatsAsync(IReadOnlyCollection<int> solvedMissionIds, int limit, CancellationToken cancellationToken = default)
    {
        if (solvedMissionIds.Count == 0)
            return Array.Empty<TagSolvedProjection>();

        var missionIdList = solvedMissionIds.ToList();

        var items = await _dbContext.MissionTags
            .AsNoTracking()
            .Where(mt => missionIdList.Contains(mt.MissionId) && !mt.Mission.IsDeleted)
            .GroupBy(mt => new { mt.TagId, mt.Tag.Name })
            .Select(g => new TagSolvedProjection(
                g.Key.TagId,
                g.Key.Name,
                g.Select(mt => mt.MissionId).Distinct().Count()))
            .OrderByDescending(x => x.SolvedCount)
            .ThenBy(x => x.TagName)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return items;
    }

    public async Task<IReadOnlyList<TagTotalProjection>> GetTagTotalsAsync(IReadOnlyCollection<int> tagIds, CancellationToken cancellationToken = default)
    {
        if (tagIds.Count == 0)
            return Array.Empty<TagTotalProjection>();

        var tagIdList = tagIds.ToList();

        var items = await _dbContext.MissionTags
            .AsNoTracking()
            .Where(mt => tagIdList.Contains(mt.TagId) && !mt.Mission.IsDeleted)
            .GroupBy(mt => mt.TagId)
            .Select(g => new TagTotalProjection(
                g.Key,
                g.Select(mt => mt.MissionId).Distinct().Count()))
            .ToListAsync(cancellationToken);

        return items;
    }

    public async Task<IReadOnlyList<SubmissionProjection>> GetRecentSubmissionsAsync(int userId, int limit, CancellationToken cancellationToken = default)
    {
        var items = await _dbContext.UserSubmits
            .AsNoTracking()
            .Where(s => !s.IsDeleted &&
                        s.User.Id == userId &&
                        !s.Solution.Mission.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .Take(limit)
            .Select(s => new SubmissionProjection(
                s.Solution.Mission.Id,
                s.Solution.Mission.Name,
                s.Solution.Mission.Difficulty,
                s.Solution.Status ?? string.Empty,
                s.CreatedAt,
                s.Solution.Mission.TimeLimitMilliseconds,
                s.Solution.Mission.MemoryLimitBytes))
            .ToListAsync(cancellationToken);

        return items;
    }

    public async Task<IReadOnlyList<AuthoredMissionProjection>> GetAuthoredMissionsAsync(int userId, int limit, CancellationToken cancellationToken = default)
    {
        var items = await _dbContext.Missions
            .AsNoTracking()
            .Where(m => !m.IsDeleted && EF.Property<int>(m, "AuthorId") == userId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .Select(m => new AuthoredMissionProjection(
                m.Id,
                m.Name,
                m.Difficulty,
                m.CreatedAt,
                m.TimeLimitMilliseconds,
                m.MemoryLimitBytes))
            .ToListAsync(cancellationToken);

        return items;
    }

    public async Task<ContestActivityMetrics> GetContestActivityAsync(int userId, DateTime fromUtc, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ContestAttempts
            .AsNoTracking()
            .Where(a => a.UserId == userId);

        var total = await query.CountAsync(cancellationToken);
        var recent = await query
            .Where(a => a.CreatedAt >= fromUtc)
            .CountAsync(cancellationToken);

        return new ContestActivityMetrics(total, recent);
    }

    public async Task<CreationActivityMetrics> GetCreationActivityAsync(int userId, DateTime fromUtc, CancellationToken cancellationToken = default)
    {
        var missionsTotal = await _dbContext.Missions
            .AsNoTracking()
            .CountAsync(m => !m.IsDeleted && EF.Property<int>(m, "AuthorId") == userId, cancellationToken);

        var missionsRecent = await _dbContext.Missions
            .AsNoTracking()
            .CountAsync(m => !m.IsDeleted && EF.Property<int>(m, "AuthorId") == userId && m.CreatedAt >= fromUtc, cancellationToken);

        var articlesTotal = await _dbContext.Articles
            .AsNoTracking()
            .CountAsync(a => !a.IsDeleted && a.AuthorId == userId, cancellationToken);

        var articlesRecent = await _dbContext.Articles
            .AsNoTracking()
            .CountAsync(a => !a.IsDeleted && a.AuthorId == userId && a.CreatedAt >= fromUtc, cancellationToken);

        var organizerMemberships = _dbContext.ContestMemberships
            .AsNoTracking()
            .Where(m => m.UserId == userId &&
                        (m.Role & ContestMembershipRole.Organizer) != 0 &&
                        !m.Contest.IsDeleted);

        var contestsTotal = await organizerMemberships
            .Select(m => m.ContestId)
            .Distinct()
            .CountAsync(cancellationToken);

        var contestsRecent = await organizerMemberships
            .Where(m => m.CreatedAt >= fromUtc)
            .Select(m => m.ContestId)
            .Distinct()
            .CountAsync(cancellationToken);

        return new CreationActivityMetrics(missionsTotal, missionsRecent, articlesTotal, articlesRecent, contestsTotal, contestsRecent);
    }

}
