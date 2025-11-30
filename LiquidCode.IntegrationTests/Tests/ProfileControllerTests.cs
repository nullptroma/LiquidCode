using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LiquidCode.Api.Contests.Requests;
using LiquidCode.Api.Contests.Responses;
using LiquidCode.Api.Profile.Responses;
using LiquidCode.Infrastructure.Database;
using LiquidCode.Infrastructure.Database.Entities;
using LiquidCode.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Sdk;

[Collection(IntegrationTestCollection.Name)]
public class ProfileControllerTests
{
    private readonly IntegrationTestFixture _fixture;
    private static JsonSerializerOptions JsonOptions => TestJsonOptions.Default;
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public ProfileControllerTests(IntegrationTestFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task GetOverview_ReturnsCreationAndContestStats()
    {
        var owner = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "profile_overview_owner");

        await TestClientHelper.CreateMissionInDatabaseAsync(_fixture, owner.UserId);
        await TestClientHelper.CreateMissionInDatabaseAsync(_fixture, owner.UserId);
        await TestClientHelper.CreateArticleInDatabaseAsync(_fixture, owner.UserId);
        await CreateContestAsync(owner, ContestScheduleType.AlwaysOpen, ContestVisibility.Public, null, null);

        var otherOrganizer = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "profile_overview_other");
        var start = DateTime.UtcNow.AddMinutes(-5);
        var participationContest = await CreateContestAsync(
            otherOrganizer,
            ContestScheduleType.RollingWindow,
            ContestVisibility.Public,
            start,
            start.AddHours(1));

        await JoinContestAsync(owner.Client, participationContest.Id);
        var attemptResponse = await owner.Client.PostAsync($"contests/{participationContest.Id}/attempts", null, Ct);
        Assert.Equal(HttpStatusCode.OK, attemptResponse.StatusCode);

        var response = await owner.Client.GetAsync($"profile/{owner.Username}", Ct);
        await EnsureSuccessAsync(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var overview = await ReadAsync<ProfileOverviewResponse>(response);

        Assert.Equal(owner.UserId, overview.Identity.UserId);
        Assert.Equal(owner.Username, overview.Identity.Username);
        Assert.Equal(3, overview.Creation.Missions.Total);
        Assert.Equal(3, overview.Creation.Missions.Last7Days);
        Assert.Equal(1, overview.Creation.Articles.Total);
        Assert.Equal(1, overview.Creation.Articles.Last7Days);
        Assert.Equal(1, overview.Creation.Contests.Total);
        Assert.Equal(1, overview.Creation.Contests.Last7Days);
        Assert.Equal(1, overview.Contests.TotalParticipations);
        Assert.Equal(1, overview.Contests.ParticipationsLast7Days);
    }

    [Fact]
    public async Task GetMissions_ReturnsPagedAuthoredMissions()
    {
        var owner = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "profile_missions_owner");

        var missionNames = new List<(int Id, string Name)>
        {
            (await TestClientHelper.CreateMissionInDatabaseAsync(_fixture, owner.UserId, "Mission Easy", difficulty: 500), "Mission Easy"),
            (await TestClientHelper.CreateMissionInDatabaseAsync(_fixture, owner.UserId, "Mission Medium", difficulty: 1500), "Mission Medium"),
            (await TestClientHelper.CreateMissionInDatabaseAsync(_fixture, owner.UserId, "Mission Hard", difficulty: 2600), "Mission Hard")
        };

        var response = await owner.Client.GetAsync($"profile/{owner.Username}/missions?recentPageSize=5&authoredPageSize=2", Ct);
        await EnsureSuccessAsync(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var missions = await ReadAsync<ProfileProblemsResponse>(response);

        Assert.Empty(missions.Recent.Items);
        Assert.Equal(2, missions.Authored.PageSize);
        Assert.True(missions.Authored.HasNextPage);
        Assert.Equal(2, missions.Authored.Items.Count);
        var returnedIds = missions.Authored.Items.Select(a => a.MissionId).ToList();
        Assert.Contains(missionNames[2].Id, returnedIds);
        Assert.Contains(missionNames[1].Id, returnedIds);
        Assert.DoesNotContain(missionNames[0].Id, returnedIds);
    }

    [Fact]
    public async Task GetArticles_ReturnsPagedArticles()
    {
        var owner = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "profile_articles_owner");

        var articleNames = new List<string>
        {
            "Article Alpha",
            "Article Beta",
            "Article Gamma"
        };

        foreach (var name in articleNames)
        {
            await TestClientHelper.CreateArticleInDatabaseAsync(_fixture, owner.UserId, name);
        }

        var response = await owner.Client.GetAsync($"profile/{owner.Username}/articles?pageSize=2&page=0", Ct);
        await EnsureSuccessAsync(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var articles = await ReadAsync<ProfileArticlesResponse>(response);

        Assert.Equal(2, articles.Articles.PageSize);
        Assert.True(articles.Articles.HasNextPage);
        Assert.Equal(2, articles.Articles.Items.Count);
        Assert.Contains(articles.Articles.Items, a => a.Title == "Article Gamma");
        Assert.Contains(articles.Articles.Items, a => a.Title == "Article Beta");
    }

    [Fact]
    public async Task GetContests_ReturnsUpcomingPastAndMine()
    {
        var owner = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "profile_contests_owner");

        var mineContest = await CreateContestAsync(
            owner,
            ContestScheduleType.AlwaysOpen,
            ContestVisibility.Public,
            null,
            null);

        var upcomingOrganizer = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "profile_contests_upcoming_org");
        var upcomingStart = DateTime.UtcNow.AddMinutes(-10);
        var upcomingContest = await CreateContestAsync(
            upcomingOrganizer,
            ContestScheduleType.RollingWindow,
            ContestVisibility.Public,
            upcomingStart,
            upcomingStart.AddHours(2));
        await JoinContestAsync(owner.Client, upcomingContest.Id);

        var pastOrganizer = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "profile_contests_past_org");
        var pastStart = DateTime.UtcNow.AddHours(-2);
        var pastContest = await CreateContestAsync(
            pastOrganizer,
            ContestScheduleType.FixedWindow,
            ContestVisibility.Public,
            pastStart,
            pastStart.AddHours(2));
        await JoinContestAsync(owner.Client, pastContest.Id);
        await UpdateContestTimesAsync(pastContest.Id, endsAt: DateTime.UtcNow.AddMinutes(-5));

        var response = await owner.Client.GetAsync(
            $"profile/{owner.Username}/contests?upcomingPageSize=1&pastPageSize=1&minePageSize=1",
            Ct);
        await EnsureSuccessAsync(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var contests = await ReadAsync<ProfileContestsResponse>(response);

        Assert.Single(contests.Mine.Items);
        Assert.Equal(mineContest.Id, contests.Mine.Items[0].ContestId);
        Assert.True(contests.Mine.Items[0].Role.HasFlag(ContestMembershipRole.Organizer));

        Assert.Single(contests.Upcoming.Items);
        Assert.Equal(upcomingContest.Id, contests.Upcoming.Items[0].ContestId);
        Assert.True(contests.Upcoming.Items[0].Role.HasFlag(ContestMembershipRole.Participant));

        Assert.Single(contests.Past.Items);
        Assert.Equal(pastContest.Id, contests.Past.Items[0].ContestId);
        Assert.True(contests.Past.Items[0].Role.HasFlag(ContestMembershipRole.Participant));
    }

    private async Task<ContestResponse> CreateContestAsync(
        AuthenticatedUser organizer,
        ContestScheduleType scheduleType,
        ContestVisibility visibility,
        DateTime? startsAt,
        DateTime? endsAt,
        int attemptDurationMinutes = 30,
        int maxAttempts = 3)
    {
        var missionId = await TestClientHelper.CreateMissionInDatabaseAsync(_fixture, organizer.UserId);
        var request = new CreateContestRequest(
            $"Contest {Guid.NewGuid():N}",
            "Profile integration test contest",
            scheduleType,
            visibility,
            startsAt,
            endsAt,
            attemptDurationMinutes,
            maxAttempts,
            true,
            null,
            new[] { missionId },
            null);

        var response = await organizer.Client.PostAsJsonAsync("contests", request, Ct);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await ReadAsync<ContestResponse>(response);
    }

    private static async Task JoinContestAsync(HttpClient client, int contestId)
    {
        var response = await client.PostAsJsonAsync(
            $"contests/{contestId}/members",
            new ContestMembershipRequest(null, null),
            Ct);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task UpdateContestTimesAsync(int contestId, DateTime? startsAt = null, DateTime? endsAt = null)
    {
        await using var scope = _fixture.Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<LiquidDbContext>();
        var contest = await context.Contests.FirstAsync(c => c.Id == contestId, Ct);
        if (startsAt.HasValue)
        {
            contest.StartsAt = startsAt;
        }

        if (endsAt.HasValue)
        {
            contest.EndsAt = endsAt;
        }

        await context.SaveChangesAsync(Ct);
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        var payload = await response.Content.ReadFromJsonAsync<T>(JsonOptions, Ct);
        Assert.NotNull(payload);
        return payload!;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var content = await response.Content.ReadAsStringAsync(Ct);
        throw new XunitException($"Expected success but got {(int)response.StatusCode} {response.StatusCode}: {content}");
    }
}
