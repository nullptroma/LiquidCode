using System;
using System.Collections.Generic;
using System.Linq;
using LiquidCode.Api.Profile.Responses;
using LiquidCode.Domain.Interfaces.Repositories;
using LiquidCode.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace LiquidCode.Domain.Services.Profile;

public class ProfileService(
    IUserRepository userRepository,
    IProfileRepository profileRepository,
    ILogger<ProfileService> logger) : IProfileService
{
    private const string AcceptedStatusPrefix = "Accepted";
    private const int EasyDifficultyMax = 1200;
    private const int MediumDifficultyMax = 2000;
    private const int CompetencyLimit = 10;
    private const int RecentSubmissionsLimit = 10;
    private const int AuthoredMissionsLimit = 10;

    public async Task<ProfileDetailsResponse?> GetProfileAsync(string username, int? requesterId, CancellationToken cancellationToken = default)
    {
        _ = requesterId;

        var user = await userRepository.FindByUsernameAsync(username, cancellationToken);
        if (user == null || user.IsDeleted)
            return null;

        try
        {
            var userId = user.Id;
            var now = DateTime.UtcNow;
            var last7Days = now.AddDays(-7);

            var placement = await profileRepository.GetUserPlacementAsync(userId, cancellationToken);

            double? topPercent = null;
            if (placement.TotalUsers > 0)
            {
                var position = placement.TotalUsers - placement.HigherAcceptedUsersCount;
                topPercent = Math.Round(position * 100d / placement.TotalUsers, 1);
            }

            var solvedMissions = await profileRepository.GetSolvedMissionsAsync(userId, cancellationToken);
            var solvedMissionIds = solvedMissions.Select(m => m.MissionId).ToList();
            var solvedMissionCount = solvedMissionIds.Count;
            var solvedByBucket = solvedMissions
                .GroupBy(m => GetDifficultyBucket(m.Difficulty))
                .ToDictionary(g => g.Key, g => g.Count());

            var solvedLast7Days = await profileRepository.CountSolvedMissionsSinceAsync(userId, last7Days, cancellationToken);

            var missionDifficultyTotals = await profileRepository.GetMissionDifficultyTotalsAsync(EasyDifficultyMax, MediumDifficultyMax, cancellationToken);
            var bucketTotals = missionDifficultyTotals
                .Select(item => new { Bucket = ToDifficultyBucket(item.BucketKey), item.Count })
                .GroupBy(item => item.Bucket)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Count));

            var recentSubmissions = await profileRepository.GetRecentSubmissionsAsync(userId, RecentSubmissionsLimit, cancellationToken);
            var recentSubmissionResponses = recentSubmissions
                .Select(item => new ProfileMissionActivityItemResponse(
                    item.MissionId,
                    item.MissionName,
                    GetDifficultyLabel(item.Difficulty),
                    item.Difficulty,
                    item.Status.StartsWith(AcceptedStatusPrefix, StringComparison.OrdinalIgnoreCase),
                    item.Status,
                    item.CreatedAt,
                    item.TimeLimitMilliseconds,
                    item.MemoryLimitBytes))
                .ToList();

            var authoredMissions = await profileRepository.GetAuthoredMissionsAsync(userId, AuthoredMissionsLimit, cancellationToken);
            var authoredMissionResponses = authoredMissions
                .Select(m => new ProfileAuthoredMissionResponse(
                    m.MissionId,
                    m.MissionName,
                    GetDifficultyLabel(m.Difficulty),
                    m.Difficulty,
                    m.CreatedAt,
                    m.TimeLimitMilliseconds,
                    m.MemoryLimitBytes))
                .ToList();

            var competencies = new List<ProfileCompetencyResponse>();
            if (solvedMissionIds.Count > 0)
            {
                var competencyStats = await profileRepository.GetCompetencyStatsAsync(solvedMissionIds, CompetencyLimit, cancellationToken);
                if (competencyStats.Count > 0)
                {
                    var tagTotals = await profileRepository.GetTagTotalsAsync(competencyStats.Select(x => x.TagId).ToList(), cancellationToken);
                    var totalsMap = tagTotals.ToDictionary(x => x.TagId, x => x.TotalCount);

                    competencies = competencyStats
                        .Select(x => new ProfileCompetencyResponse(
                            x.TagName,
                            x.SolvedCount,
                            totalsMap.TryGetValue(x.TagId, out var total) ? total : x.SolvedCount))
                        .ToList();
                }
            }

            var contestActivity = await profileRepository.GetContestActivityAsync(userId, last7Days, cancellationToken);
            var creationActivity = await profileRepository.GetCreationActivityAsync(userId, last7Days, cancellationToken);

            var totalMissions = bucketTotals.Values.Sum();

            var difficultyProgress = new List<ProfileDifficultyProgressResponse>
            {
                BuildDifficultyProgress(DifficultyBucket.Easy, bucketTotals, solvedByBucket),
                BuildDifficultyProgress(DifficultyBucket.Medium, bucketTotals, solvedByBucket),
                BuildDifficultyProgress(DifficultyBucket.Hard, bucketTotals, solvedByBucket)
            };

            return new ProfileDetailsResponse(
                new ProfileHeaderResponse(
                    user.Id,
                    user.Username,
                    user.Email,
                    user.CreatedAt,
                    topPercent,
                    null),
                new ProfileProblemProgressResponse(
                    new ProfileProgressCounterResponse("total", "Задачи", solvedMissionCount, totalMissions),
                    difficultyProgress),
                competencies,
                recentSubmissionResponses,
                authoredMissionResponses,
                new ProfileActivityResponse(
                    new ProfileSolutionActivityResponse(
                        new ProfileActivityMetricResponse("Задачи", solvedMissionCount, solvedLast7Days),
                        new ProfileActivityMetricResponse("Контесты", contestActivity.TotalAttempts, contestActivity.AttemptsLastPeriod)),
                    new ProfileCreationActivityResponse(
                        new ProfileActivityMetricResponse("Задачи", creationActivity.MissionsTotal, creationActivity.MissionsLastPeriod),
                        new ProfileActivityMetricResponse("Статьи", creationActivity.ArticlesTotal, creationActivity.ArticlesLastPeriod),
                        new ProfileActivityMetricResponse("Контесты", creationActivity.ContestsTotal, creationActivity.ContestsLastPeriod))));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to build profile for user {Username}", username);
            return null;
        }
    }

    private static DifficultyBucket GetDifficultyBucket(int difficulty) => difficulty <= EasyDifficultyMax
        ? DifficultyBucket.Easy
        : difficulty <= MediumDifficultyMax
            ? DifficultyBucket.Medium
            : DifficultyBucket.Hard;

    private static DifficultyBucket ToDifficultyBucket(int key) => key switch
    {
        0 => DifficultyBucket.Easy,
        1 => DifficultyBucket.Medium,
        _ => DifficultyBucket.Hard
    };

    private static string GetDifficultyLabel(int difficulty) => GetDifficultyLabel(GetDifficultyBucket(difficulty));

    private static string GetDifficultyLabel(DifficultyBucket bucket) => bucket switch
    {
        DifficultyBucket.Easy => "Easy",
        DifficultyBucket.Medium => "Medium",
        _ => "Hard"
    };

    private static ProfileDifficultyProgressResponse BuildDifficultyProgress(
        DifficultyBucket bucket,
        IReadOnlyDictionary<DifficultyBucket, int> totals,
        IReadOnlyDictionary<DifficultyBucket, int> solved)
    {
        var key = bucket switch
        {
            DifficultyBucket.Easy => "easy",
            DifficultyBucket.Medium => "medium",
            _ => "hard"
        };

        totals.TryGetValue(bucket, out var totalCount);
        solved.TryGetValue(bucket, out var solvedCount);

        return new ProfileDifficultyProgressResponse(key, GetDifficultyLabel(bucket), solvedCount, totalCount);
    }

    private enum DifficultyBucket
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }
}
