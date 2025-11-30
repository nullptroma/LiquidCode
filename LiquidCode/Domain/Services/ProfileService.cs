using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiquidCode.Api.Profile.Responses;
using LiquidCode.Domain.Interfaces.Repositories;
using LiquidCode.Domain.Interfaces.Services;
using LiquidCode.Infrastructure.Database.Entities;
using LiquidCode.Shared.Constants;
using Microsoft.Extensions.Logging;

namespace LiquidCode.Domain.Services.Profile;

public class ProfileService(
    IUserRepository userRepository,
    IProfileRepository profileRepository,
    ILogger<ProfileService> logger) : IProfileService
{
    public async Task<ProfileOverviewResponse?> GetOverviewAsync(string username, int? requesterId, CancellationToken cancellationToken = default)
    {
        _ = requesterId;

        var user = await FindActiveUserAsync(username, cancellationToken);
        if (user == null)
            return null;

        try
        {
            var userId = user.Id;
            var now = DateTime.UtcNow;
            var last7Days = now.AddDays(-7);

            var solvedMissions = await profileRepository.GetSolvedMissionsAsync(userId, cancellationToken);
            var solvedCount = solvedMissions.Count;
            var solvedLast7Days = await profileRepository.CountSolvedMissionsSinceAsync(userId, last7Days, cancellationToken);

            var contestActivity = await profileRepository.GetContestActivityAsync(userId, last7Days, cancellationToken);
            var creationActivity = await profileRepository.GetCreationActivityAsync(userId, last7Days, cancellationToken);

            return new ProfileOverviewResponse(
                new ProfileIdentityResponse(user.Id, user.Username, user.Email, user.CreatedAt),
                new ProfileSolutionsStatsResponse(solvedCount, solvedLast7Days),
                new ProfileContestStatsResponse(contestActivity.TotalAttempts, contestActivity.AttemptsLastPeriod),
                new ProfileCreationStatsResponse(
                    new ProfileMetricResponse(creationActivity.MissionsTotal, creationActivity.MissionsLastPeriod),
                    new ProfileMetricResponse(creationActivity.ContestsTotal, creationActivity.ContestsLastPeriod),
                    new ProfileMetricResponse(creationActivity.ArticlesTotal, creationActivity.ArticlesLastPeriod)));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load overview for user {Username}", username);
            return null;
        }
    }

    public async Task<ProfileProblemsResponse?> GetProblemsAsync(
        string username,
        int? requesterId,
        ProfileProblemsQuery query,
        CancellationToken cancellationToken = default)
    {
        _ = requesterId;

        var user = await FindActiveUserAsync(username, cancellationToken);
        if (user == null)
            return null;

        try
        {
            var solvedMissions = await profileRepository.GetSolvedMissionsAsync(user.Id, cancellationToken);
            var solvedByBucket = solvedMissions
                .GroupBy(m => GetDifficultyBucket(m.Difficulty))
                .ToDictionary(g => g.Key, g => g.Count());

            var missionDifficultyTotals = await profileRepository.GetMissionDifficultyTotalsAsync(
                MissionDifficultyThresholds.EasyMax,
                MissionDifficultyThresholds.MediumMax,
                cancellationToken);

            var bucketTotals = missionDifficultyTotals
                .Select(item => new { Bucket = ToDifficultyBucket(item.BucketKey), item.Count })
                .GroupBy(item => item.Bucket)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Count));

            var totalMissions = bucketTotals.Values.Sum();
            var summary = new ProfileProblemsSummaryResponse(
                new ProfileProblemCounterResponse("total", "Задачи", solvedMissions.Count, totalMissions),
                new List<ProfileProblemCounterResponse>
                {
                    BuildBucketCounter(DifficultyBucket.Easy, bucketTotals, solvedByBucket),
                    BuildBucketCounter(DifficultyBucket.Medium, bucketTotals, solvedByBucket),
                    BuildBucketCounter(DifficultyBucket.Hard, bucketTotals, solvedByBucket)
                });

            var recent = await profileRepository.GetRecentMissionActivitiesAsync(
                user.Id,
                query.RecentPageSize,
                query.RecentPage,
                cancellationToken);

            var recentResponses = recent.Items
                .Select(ToRecentMissionResponse)
                .ToList();

            var authored = await profileRepository.GetAuthoredMissionsPageAsync(
                user.Id,
                query.AuthoredPageSize,
                query.AuthoredPage,
                cancellationToken);

            var authoredResponses = authored.Items
                .Select(m => new ProfileAuthoredMissionResponse(
                    m.MissionId,
                    m.MissionName,
                    GetDifficultyLabel(GetDifficultyBucket(m.Difficulty)),
                    m.Difficulty,
                    m.CreatedAt,
                    m.TimeLimitMilliseconds,
                    m.MemoryLimitBytes))
                .ToList();

            return new ProfileProblemsResponse(
                summary,
                BuildPagedResponse(recentResponses, query.RecentPage, query.RecentPageSize, recent.HasNextPage),
                BuildPagedResponse(authoredResponses, query.AuthoredPage, query.AuthoredPageSize, authored.HasNextPage));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load missions page for user {Username}", username);
            return null;
        }
    }

    public async Task<ProfileArticlesResponse?> GetArticlesAsync(
        string username,
        int? requesterId,
        ProfileArticlesQuery query,
        CancellationToken cancellationToken = default)
    {
        _ = requesterId;

        var user = await FindActiveUserAsync(username, cancellationToken);
        if (user == null)
            return null;

        try
        {
            var page = await profileRepository.GetArticlesPageAsync(
                user.Id,
                query.PageSize,
                query.Page,
                cancellationToken);

            var items = page.Items
                .Select(a => new ProfileArticleResponse(a.ArticleId, a.Title, a.CreatedAt, a.UpdatedAt))
                .ToList();

            return new ProfileArticlesResponse(
                BuildPagedResponse(items, query.Page, query.PageSize, page.HasNextPage));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load articles page for user {Username}", username);
            return null;
        }
    }

    public async Task<ProfileContestsResponse?> GetContestsAsync(
        string username,
        int? requesterId,
        ProfileContestsQuery query,
        CancellationToken cancellationToken = default)
    {
        _ = requesterId;

        var user = await FindActiveUserAsync(username, cancellationToken);
        if (user == null)
            return null;

        try
        {
            var upcoming = await profileRepository.GetUserContestsPageAsync(
                user.Id,
                ProfileContestFilter.Upcoming,
                query.UpcomingPageSize,
                query.UpcomingPage,
                cancellationToken);

            var past = await profileRepository.GetUserContestsPageAsync(
                user.Id,
                ProfileContestFilter.Past,
                query.PastPageSize,
                query.PastPage,
                cancellationToken);

            var mine = await profileRepository.GetUserContestsPageAsync(
                user.Id,
                ProfileContestFilter.Organized,
                query.MinePageSize,
                query.MinePage,
                cancellationToken);

            return new ProfileContestsResponse(
                BuildPagedResponse(upcoming.Items.Select(ToContestResponse).ToList(), query.UpcomingPage, query.UpcomingPageSize, upcoming.HasNextPage),
                BuildPagedResponse(past.Items.Select(ToContestResponse).ToList(), query.PastPage, query.PastPageSize, past.HasNextPage),
                BuildPagedResponse(mine.Items.Select(ToContestResponse).ToList(), query.MinePage, query.MinePageSize, mine.HasNextPage));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load contests page for user {Username}", username);
            return null;
        }
    }

    private async Task<DbUser?> FindActiveUserAsync(string username, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindByUsernameAsync(username, cancellationToken);
        return user == null || user.IsDeleted ? null : user;
    }

    private static ProfileContestResponse ToContestResponse(ProfileContestProjection projection) => new(
        projection.ContestId,
        projection.Name,
        projection.ScheduleType,
        projection.Visibility,
        projection.StartsAt,
        projection.EndsAt,
        projection.AttemptDurationMinutes,
        projection.Role);

    private static ProfileRecentMissionResponse ToRecentMissionResponse(ProfileRecentMissionProjection projection)
    {
        var submission = projection.LatestAccepted ?? projection.LatestSubmission;
        var bucket = GetDifficultyBucket(projection.Difficulty);
        return new ProfileRecentMissionResponse(
            projection.MissionId,
            projection.MissionName,
            GetDifficultyLabel(bucket),
            projection.Difficulty,
            submission.IsAccepted,
            submission.Status,
            submission.CreatedAt,
            submission.TimeLimitMilliseconds,
            submission.MemoryLimitBytes);
    }

    private static ProfileProblemCounterResponse BuildBucketCounter(
        DifficultyBucket bucket,
        IReadOnlyDictionary<DifficultyBucket, int> totals,
        IReadOnlyDictionary<DifficultyBucket, int> solved)
    {
        totals.TryGetValue(bucket, out var totalCount);
        solved.TryGetValue(bucket, out var solvedCount);

        var key = bucket switch
        {
            DifficultyBucket.Easy => "easy",
            DifficultyBucket.Medium => "medium",
            _ => "hard"
        };

        return new ProfileProblemCounterResponse(key, GetDifficultyLabel(bucket), solvedCount, totalCount);
    }

    private static ProfilePagedResponse<T> BuildPagedResponse<T>(IReadOnlyList<T> items, int page, int pageSize, bool hasNext) =>
        new(items, page, pageSize, hasNext);

    private static DifficultyBucket GetDifficultyBucket(int difficulty) => difficulty <= MissionDifficultyThresholds.EasyMax
        ? DifficultyBucket.Easy
        : difficulty <= MissionDifficultyThresholds.MediumMax
            ? DifficultyBucket.Medium
            : DifficultyBucket.Hard;

    private static DifficultyBucket ToDifficultyBucket(int key) => key switch
    {
        0 => DifficultyBucket.Easy,
        1 => DifficultyBucket.Medium,
        _ => DifficultyBucket.Hard
    };

    private static string GetDifficultyLabel(DifficultyBucket bucket) => bucket switch
    {
        DifficultyBucket.Easy => "Easy",
        DifficultyBucket.Medium => "Medium",
        _ => "Hard"
    };

    private enum DifficultyBucket
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }
}
