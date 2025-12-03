using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LiquidCode.Api.Contests.Requests;
using LiquidCode.Api.Contests.Responses;
using LiquidCode.Api.Submits.Dto;
using LiquidCode.Api.Submits.Responses;
using LiquidCode.Infrastructure.Database;
using LiquidCode.Infrastructure.Database.Entities;
using LiquidCode.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

[Collection(IntegrationTestCollection.Name)]
public class ContestsControllerTests
{
    private readonly IntegrationTestFixture _fixture;
    private static JsonSerializerOptions JsonOptions => TestJsonOptions.Default;
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public ContestsControllerTests(IntegrationTestFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task CreateAlwaysOpenPublicContest_ReturnsContestWithLineup()
    {
        var organizer = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_create_public");

        var (contest, missionIds) = await CreateContestAsync(
            organizer,
            ContestScheduleType.AlwaysOpen,
            ContestVisibility.Public,
            startsAt: null,
            endsAt: null,
            attemptDurationMinutes: 120,
            maxAttempts: 5,
            includeArticle: true);

        Assert.Equal(ContestScheduleType.AlwaysOpen, contest.ScheduleType);
        Assert.Null(contest.StartsAt);
        Assert.Null(contest.EndsAt);
        Assert.Equal(120, contest.AttemptDurationMinutes);
        Assert.Equal(ContestVisibility.Public, contest.Visibility);
        Assert.Single(contest.Missions);
        Assert.Equal(missionIds[0], contest.Missions[0].Id);
        Assert.Single(contest.Articles);

        var getResponse = await organizer.Client.GetAsync($"contests/{contest.Id}", Ct);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetched = await ReadAsync<ContestResponse>(getResponse);
        Assert.Equal(contest.Id, fetched.Id);
    }

    [Fact]
    public async Task CreateGroupPrivateContest_ByNonAdmin_ReturnsBadRequest()
    {
        var owner = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_create_group_owner");
        var group = await TestClientHelper.CreateGroupWithUniqueNameAsync(owner.Client);
        var nonAdmin = await TestClientHelper.CreateGroupMemberAsync(_fixture, owner.Client, group.GroupId, "contest_create_group_member");
        var missionId = await TestClientHelper.CreateMissionInDatabaseAsync(_fixture, nonAdmin.UserId);

        var now = DateTime.UtcNow;
        var request = new CreateContestRequest(
            $"Group Private {Guid.NewGuid():N}",
            "Private contest",
            ContestScheduleType.FixedWindow,
            ContestVisibility.GroupPrivate,
            now,
            now.AddHours(2),
            90,
            1,
            true,
            group.GroupId,
            new[] { missionId },
            null);

        var response = await nonAdmin.Client.PostAsJsonAsync("contests", request, Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var message = await response.Content.ReadAsStringAsync(Ct);
        Assert.Contains("Only group administrators", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateContest_SwitchesToRollingWindowAndGroup()
    {
        var organizer = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_update_owner");
        var start = DateTime.UtcNow.AddMinutes(-5);
        var end = start.AddHours(1);
        var (contest, _) = await CreateContestAsync(
            organizer,
            ContestScheduleType.FixedWindow,
            ContestVisibility.Public,
            startsAt: start,
            endsAt: end,
            attemptDurationMinutes: 60,
            maxAttempts: 1);

        var group = await TestClientHelper.CreateGroupWithUniqueNameAsync(organizer.Client);
        var newMissionId = await TestClientHelper.CreateMissionInDatabaseAsync(_fixture, organizer.UserId);
        var updateRequest = new UpdateContestRequest(
            $"Updated {contest.Name}",
            "Updated description",
            ContestScheduleType.RollingWindow,
            ContestVisibility.GroupPrivate,
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(4),
            45,
            3,
            false,
            group.GroupId,
            new[] { newMissionId },
            null);

        var response = await organizer.Client.PutAsJsonAsync($"contests/{contest.Id}", updateRequest, Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await ReadAsync<ContestResponse>(response);

        Assert.Equal(ContestScheduleType.RollingWindow, updated.ScheduleType);
        Assert.Equal(group.GroupId, updated.GroupId);
        Assert.Equal(45, updated.AttemptDurationMinutes);
        Assert.Equal(3, updated.MaxAttempts);
        Assert.False(updated.AllowEarlyFinish);
        Assert.Single(updated.Missions);
        Assert.Equal(newMissionId, updated.Missions[0].Id);
    }

    [Fact]
    public async Task DeleteContest_AsOrganizer_RemovesContest()
    {
        var organizer = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_delete_owner");
        var (contest, _) = await CreateContestAsync(
            organizer,
            ContestScheduleType.AlwaysOpen,
            ContestVisibility.Public,
            startsAt: null,
            endsAt: null,
            attemptDurationMinutes: 30,
            maxAttempts: 2);

        var deleteResponse = await organizer.Client.DeleteAsync($"contests/{contest.Id}", Ct);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await organizer.Client.GetAsync($"contests/{contest.Id}", Ct);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteContest_ByNonOrganizer_ReturnsNotFound()
    {
        var organizer = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_delete_owner2");
        var (contest, _) = await CreateContestAsync(
            organizer,
            ContestScheduleType.FixedWindow,
            ContestVisibility.Public,
            DateTime.UtcNow.AddMinutes(-5),
            DateTime.UtcNow.AddHours(1),
            attemptDurationMinutes: 60,
            maxAttempts: 1);

        var stranger = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_delete_stranger");
        var response = await stranger.Client.DeleteAsync($"contests/{contest.Id}", Ct);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpsertMember_SelfRegistration_PublicContest()
    {
        var organizer = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_member_owner");
        var (contest, _) = await CreateContestAsync(
            organizer,
            ContestScheduleType.AlwaysOpen,
            ContestVisibility.Public,
            null,
            null,
            attemptDurationMinutes: 25,
            maxAttempts: 3);

        var participant = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_member_participant");

        var initialRegisteredResponse = await participant.Client.GetAsync($"contests/{contest.Id}/registered", Ct);
        var initialFlag = await ReadAsync<JsonElement>(initialRegisteredResponse);
        Assert.False(initialFlag.GetProperty("isRegistered").GetBoolean());

        var joinResponse = await participant.Client.PostAsJsonAsync(
            $"contests/{contest.Id}/members",
            new ContestMembershipRequest(null, null),
            Ct);
        Assert.Equal(HttpStatusCode.NoContent, joinResponse.StatusCode);

        var registeredResponse = await participant.Client.GetAsync($"contests/{contest.Id}/registered", Ct);
        var registeredPayload = await ReadAsync<JsonElement>(registeredResponse);
        Assert.True(registeredPayload.GetProperty("isRegistered").GetBoolean());
    }

    [Fact]
    public async Task RemoveMember_OrganizerRemovesParticipant()
    {
        var organizer = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_remove_owner");
        var (contest, _) = await CreateContestAsync(
            organizer,
            ContestScheduleType.AlwaysOpen,
            ContestVisibility.Public,
            null,
            null,
            attemptDurationMinutes: 20,
            maxAttempts: 2);

        var participant = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_remove_participant");
        var joinResponse = await participant.Client.PostAsJsonAsync(
            $"contests/{contest.Id}/members",
            new ContestMembershipRequest(null, null),
            Ct);
        Assert.Equal(HttpStatusCode.NoContent, joinResponse.StatusCode);

        var removeResponse = await organizer.Client.DeleteAsync($"contests/{contest.Id}/members/{participant.UserId}", Ct);
        Assert.Equal(HttpStatusCode.NoContent, removeResponse.StatusCode);

        var registeredResponse = await participant.Client.GetAsync($"contests/{contest.Id}/registered", Ct);
        var registeredPayload = await ReadAsync<JsonElement>(registeredResponse);
        Assert.False(registeredPayload.GetProperty("isRegistered").GetBoolean());
    }

    [Fact]
    public async Task GetContestMembers_ReturnsOrganizerFirstAndSupportsPagination()
    {
        var organizer = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_members_owner");
        var (contest, _) = await CreateContestAsync(
            organizer,
            ContestScheduleType.AlwaysOpen,
            ContestVisibility.Public,
            null,
            null,
            attemptDurationMinutes: 15,
            maxAttempts: 2);

        var participantA = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_members_a");
        var participantB = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_members_b");

        foreach (var user in new[] { participantA, participantB })
        {
            var joinResponse = await user.Client.PostAsJsonAsync(
                $"contests/{contest.Id}/members",
                new ContestMembershipRequest(null, null),
                Ct);
            Assert.Equal(HttpStatusCode.NoContent, joinResponse.StatusCode);
        }

        var page0 = await organizer.Client.GetAsync($"contests/{contest.Id}/members?pageSize=1&page=0", Ct);
        Assert.Equal(HttpStatusCode.OK, page0.StatusCode);
        var page0Payload = await ReadAsync<ContestMembersPageResponse>(page0);
        Assert.True(page0Payload.HasNextPage);
        Assert.Single(page0Payload.Members);
        var firstMember = page0Payload.Members[0];
        Assert.Equal(organizer.UserId, firstMember.UserId);
        Assert.True(firstMember.Role.HasFlag(ContestMembershipRole.Organizer));

        var page1 = await organizer.Client.GetAsync($"contests/{contest.Id}/members?pageSize=1&page=1", Ct);
        var page1Payload = await ReadAsync<ContestMembersPageResponse>(page1);
        Assert.True(page1Payload.HasNextPage);
        Assert.Single(page1Payload.Members);
        Assert.Equal(participantA.UserId, page1Payload.Members[0].UserId);
    }

    [Fact]
    public async Task StartAttempt_RollingWindowWithinSchedule_ReturnsActiveAttempt()
    {
        var organizer = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_attempts_roll_owner");
        var start = DateTime.UtcNow.AddMinutes(-5);
        var end = start.AddHours(1);
        var (contest, _) = await CreateContestAsync(
            organizer,
            ContestScheduleType.RollingWindow,
            ContestVisibility.Public,
            start,
            end,
            attemptDurationMinutes: 30,
            maxAttempts: 3);

        var participant = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_attempts_roll_participant");
        var joinResponse = await participant.Client.PostAsJsonAsync(
            $"contests/{contest.Id}/members",
            new ContestMembershipRequest(null, null),
            Ct);
        Assert.Equal(HttpStatusCode.NoContent, joinResponse.StatusCode);

        var attemptResponse = await participant.Client.PostAsync($"contests/{contest.Id}/attempts", null, Ct);
        Assert.Equal(HttpStatusCode.OK, attemptResponse.StatusCode);
        var attempt = await ReadAsync<ContestAttemptResponse>(attemptResponse);

        Assert.Equal(ContestAttemptStatus.Active, attempt.Status);
        Assert.Equal(ContestScheduleType.RollingWindow, attempt.ScheduleType);
        Assert.Equal(1, attempt.AttemptIndex);
        Assert.NotNull(attempt.ExpiresAt);
        var duration = attempt.ExpiresAt.Value - attempt.StartedAt;
        Assert.InRange(duration.TotalMinutes, 29.9, 30.1);
    }

    [Fact]
    public async Task StartAttempt_FixedWindowBeforeStart_ReturnsBadRequest()
    {
        var organizer = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_attempts_fixed_owner");
        var start = DateTime.UtcNow.AddMinutes(30);
        var end = start.AddHours(1);
        var (contest, _) = await CreateContestAsync(
            organizer,
            ContestScheduleType.FixedWindow,
            ContestVisibility.Public,
            start,
            end,
            attemptDurationMinutes: 60,
            maxAttempts: 1);

        var participant = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_attempts_fixed_participant");
        var joinResponse = await participant.Client.PostAsJsonAsync(
            $"contests/{contest.Id}/members",
            new ContestMembershipRequest(null, null),
            Ct);
        Assert.Equal(HttpStatusCode.NoContent, joinResponse.StatusCode);

        var attemptResponse = await participant.Client.PostAsync($"contests/{contest.Id}/attempts", null, Ct);
        Assert.Equal(HttpStatusCode.BadRequest, attemptResponse.StatusCode);
    }

    [Fact]
    public async Task ListContests_PublicEndpointFiltersByVisibility()
    {
        var organizer = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_list_public_owner");
        var (publicContest, _) = await CreateContestAsync(
            organizer,
            ContestScheduleType.AlwaysOpen,
            ContestVisibility.Public,
            null,
            null,
            attemptDurationMinutes: 10,
            maxAttempts: 3);

        var group = await TestClientHelper.CreateGroupWithUniqueNameAsync(organizer.Client);
        var (groupContest, _) = await CreateContestAsync(
            organizer,
            ContestScheduleType.FixedWindow,
            ContestVisibility.GroupPrivate,
            DateTime.UtcNow.AddMinutes(-5),
            DateTime.UtcNow.AddHours(1),
            attemptDurationMinutes: 15,
            maxAttempts: 1,
            groupId: group.GroupId);

        var response = await organizer.Client.GetAsync("contests?pageSize=10&page=0", Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await ReadAsync<ContestsPageResponse>(response);
        var contests = page.Contests.ToList();

        Assert.Contains(contests, c => c.Id == publicContest.Id);
        Assert.DoesNotContain(contests, c => c.Id == groupContest.Id);
        Assert.All(contests, c => Assert.Equal(ContestVisibility.Public, c.Visibility));
    }

    [Fact]
    public async Task ListContests_ForGroup_ReturnsPrivateEntries()
    {
        var organizer = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_list_group_owner");
        var group = await TestClientHelper.CreateGroupWithUniqueNameAsync(organizer.Client);
        var (contest, _) = await CreateContestAsync(
            organizer,
            ContestScheduleType.FixedWindow,
            ContestVisibility.GroupPrivate,
            DateTime.UtcNow.AddMinutes(-10),
            DateTime.UtcNow.AddHours(2),
            attemptDurationMinutes: 30,
            maxAttempts: 1,
            groupId: group.GroupId);

        var response = await organizer.Client.GetAsync($"contests?groupId={group.GroupId}&pageSize=10&page=0", Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await ReadAsync<ContestsPageResponse>(response);
        Assert.Single(page.Contests);
        Assert.Equal(contest.Id, page.Contests.First().Id);
    }

    [Fact]
    public async Task ListUpcomingEligibleContests_FiltersByRemainingAttempts()
    {
        var owner = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_eligible_owner");
        var participant = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_eligible_user");
        var now = DateTime.UtcNow;

        var (eligibleContest, _) = await CreateContestAsync(
            owner,
            ContestScheduleType.FixedWindow,
            ContestVisibility.Public,
            now.AddMinutes(-5),
            now.AddHours(2),
            attemptDurationMinutes: 45,
            maxAttempts: 2);

        var joinEligible = await participant.Client.PostAsJsonAsync(
            $"contests/{eligibleContest.Id}/members",
            new ContestMembershipRequest(null, null),
            Ct);
        Assert.Equal(HttpStatusCode.NoContent, joinEligible.StatusCode);

        var (exhaustedContest, _) = await CreateContestAsync(
            owner,
            ContestScheduleType.FixedWindow,
            ContestVisibility.Public,
            now.AddMinutes(-5),
            now.AddHours(1),
            attemptDurationMinutes: 30,
            maxAttempts: 1);

        var joinExhausted = await participant.Client.PostAsJsonAsync(
            $"contests/{exhaustedContest.Id}/members",
            new ContestMembershipRequest(null, null),
            Ct);
        Assert.Equal(HttpStatusCode.NoContent, joinExhausted.StatusCode);

        var attemptResponse = await participant.Client.PostAsync($"contests/{exhaustedContest.Id}/attempts", null, Ct);
        Assert.Equal(HttpStatusCode.OK, attemptResponse.StatusCode);

        var (endedContest, _) = await CreateContestAsync(
            owner,
            ContestScheduleType.FixedWindow,
            ContestVisibility.Public,
            now.AddHours(-3),
            now.AddHours(-1),
            attemptDurationMinutes: 30,
            maxAttempts: 2);

        var joinEnded = await participant.Client.PostAsJsonAsync(
            $"contests/{endedContest.Id}/members",
            new ContestMembershipRequest(null, null),
            Ct);
        Assert.Equal(HttpStatusCode.NoContent, joinEnded.StatusCode);

        var response = await participant.Client.GetAsync("contests/upcoming/eligible", Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var contests = await ReadAsync<List<ContestResponse>>(response);

        Assert.Contains(contests, c => c.Id == eligibleContest.Id);
        Assert.DoesNotContain(contests, c => c.Id == exhaustedContest.Id);
        Assert.DoesNotContain(contests, c => c.Id == endedContest.Id);
    }

    [Fact]
    public async Task ListMyAttempts_ReturnsAllUserAttempts()
    {
        var owner = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_my_attempts_owner");
        var participant = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_my_attempts_user");
        var now = DateTime.UtcNow;

        var (contestA, _) = await CreateContestAsync(
            owner,
            ContestScheduleType.FixedWindow,
            ContestVisibility.Public,
            now.AddMinutes(-10),
            now.AddHours(1),
            attemptDurationMinutes: 30,
            maxAttempts: 2);

        await participant.Client.PostAsJsonAsync($"contests/{contestA.Id}/members", new ContestMembershipRequest(null, null), Ct);
        var startA = await participant.Client.PostAsync($"contests/{contestA.Id}/attempts", null, Ct);
        Assert.Equal(HttpStatusCode.OK, startA.StatusCode);

        var (contestB, _) = await CreateContestAsync(
            owner,
            ContestScheduleType.RollingWindow,
            ContestVisibility.Public,
            now.AddMinutes(-5),
            now.AddHours(2),
            attemptDurationMinutes: 45,
            maxAttempts: 3);

        await participant.Client.PostAsJsonAsync($"contests/{contestB.Id}/members", new ContestMembershipRequest(null, null), Ct);
        var startB = await participant.Client.PostAsync($"contests/{contestB.Id}/attempts", null, Ct);
        Assert.Equal(HttpStatusCode.OK, startB.StatusCode);

        var response = await participant.Client.GetAsync("contests/attempts/my", Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var attempts = await ReadAsync<List<ContestAttemptDetailsResponse>>(response);

        Assert.True(attempts.Count >= 2);
        Assert.Contains(attempts, a => a.AttemptIndex == 1 && a.Status == ContestAttemptStatus.Active);
    }

    [Fact]
    public async Task GetMyActiveAttempt_ReturnsCurrentAttempt()
    {
        var owner = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_active_owner");
        var participant = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_active_user");
        var now = DateTime.UtcNow;

        var (contest, _) = await CreateContestAsync(
            owner,
            ContestScheduleType.RollingWindow,
            ContestVisibility.Public,
            now.AddMinutes(-5),
            now.AddHours(1),
            attemptDurationMinutes: 40,
            maxAttempts: 2);

        await participant.Client.PostAsJsonAsync($"contests/{contest.Id}/members", new ContestMembershipRequest(null, null), Ct);

        var startResponse = await participant.Client.PostAsync($"contests/{contest.Id}/attempts", null, Ct);
        Assert.Equal(HttpStatusCode.OK, startResponse.StatusCode);
        var startedAttempt = await ReadAsync<ContestAttemptResponse>(startResponse);

        var response = await participant.Client.GetAsync($"contests/{contest.Id}/attempts/my/active", Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var activeAttempt = await ReadAsync<ContestAttemptResponse>(response);

        Assert.Equal(startedAttempt.AttemptId, activeAttempt.AttemptId);
        Assert.Equal(ContestAttemptStatus.Active, activeAttempt.Status);
    }

    [Fact]
    public async Task GetMyActiveAttempt_ReturnsNotFoundWhenNone()
    {
        var owner = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_active_none_owner");
        var participant = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_active_none_user");
        var now = DateTime.UtcNow;

        var (contest, _) = await CreateContestAsync(
            owner,
            ContestScheduleType.FixedWindow,
            ContestVisibility.Public,
            now.AddMinutes(-5),
            now.AddHours(1),
            attemptDurationMinutes: 30,
            maxAttempts: 1);

        await participant.Client.PostAsJsonAsync($"contests/{contest.Id}/members", new ContestMembershipRequest(null, null), Ct);

        var response = await participant.Client.GetAsync($"contests/{contest.Id}/attempts/my/active", Ct);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ListEndpoints_MyAndParticipatingReturnExpectedSets()
    {
        var userA = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_list_me_a");
        var (ownedContest, _) = await CreateContestAsync(
            userA,
            ContestScheduleType.AlwaysOpen,
            ContestVisibility.Public,
            null,
            null,
            attemptDurationMinutes: 15,
            maxAttempts: 2);

        var userB = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_list_me_b");
        var (foreignContest, _) = await CreateContestAsync(
            userB,
            ContestScheduleType.FixedWindow,
            ContestVisibility.Public,
            DateTime.UtcNow.AddMinutes(-5),
            DateTime.UtcNow.AddHours(1),
            attemptDurationMinutes: 15,
            maxAttempts: 2);

        var joinResponse = await userA.Client.PostAsJsonAsync(
            $"contests/{foreignContest.Id}/members",
            new ContestMembershipRequest(null, null),
            Ct);
        Assert.Equal(HttpStatusCode.NoContent, joinResponse.StatusCode);

        var myResponse = await userA.Client.GetAsync("contests/my", Ct);
        Assert.Equal(HttpStatusCode.OK, myResponse.StatusCode);
        var myContests = await ReadAsync<List<ContestResponse>>(myResponse);
        Assert.Contains(myContests, c => c.Id == ownedContest.Id);
        Assert.DoesNotContain(myContests, c => c.Id == foreignContest.Id);

        var participatingResponse = await userA.Client.GetAsync("contests/participating?pageSize=10&page=0", Ct);
        Assert.Equal(HttpStatusCode.OK, participatingResponse.StatusCode);
        var participatingPage = await ReadAsync<ContestsPageResponse>(participatingResponse);
        Assert.Single(participatingPage.Contests);
        Assert.Equal(foreignContest.Id, participatingPage.Contests.First().Id);
    }

    [Fact]
    public async Task GetContestSubmissions_ReturnsOnlyCurrentUserEntries()
    {
        var organizer = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_submissions_owner");
        var (contest, missionIds) = await CreateContestAsync(
            organizer,
            ContestScheduleType.FixedWindow,
            ContestVisibility.Public,
            DateTime.UtcNow.AddMinutes(-5),
            DateTime.UtcNow.AddHours(1),
            attemptDurationMinutes: 20,
            maxAttempts: 2);

        var participant = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_submissions_participant");
        var joinResponse = await participant.Client.PostAsJsonAsync(
            $"contests/{contest.Id}/members",
            new ContestMembershipRequest(null, null),
            Ct);
        Assert.Equal(HttpStatusCode.NoContent, joinResponse.StatusCode);

        await SeedSubmissionAsync(contest.Id, participant.UserId, missionIds[0]);

        var submissionsResponse = await participant.Client.GetAsync($"contests/{contest.Id}/submissions/my", Ct);
        Assert.Equal(HttpStatusCode.OK, submissionsResponse.StatusCode);
        var submissions = await ReadAsync<List<SubmissionResponse>>(submissionsResponse);
        Assert.Single(submissions);
        Assert.Equal(missionIds[0], submissions[0].Solution.MissionId);
    }

    [Fact]
    public async Task GetContestAttempts_ReturnsDetailedHistory()
    {
        var organizer = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_attempt_history_owner");
        var (contest, missionIds) = await CreateContestAsync(
            organizer,
            ContestScheduleType.RollingWindow,
            ContestVisibility.Public,
            DateTime.UtcNow.AddMinutes(-5),
            DateTime.UtcNow.AddHours(2),
            attemptDurationMinutes: 40,
            maxAttempts: 3);

        var participant = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "contest_attempt_history_participant");
        var joinResponse = await participant.Client.PostAsJsonAsync(
            $"contests/{contest.Id}/members",
            new ContestMembershipRequest(null, null),
            Ct);
        Assert.Equal(HttpStatusCode.NoContent, joinResponse.StatusCode);

        var attemptResponse = await participant.Client.PostAsync($"contests/{contest.Id}/attempts", null, Ct);
        Assert.Equal(HttpStatusCode.OK, attemptResponse.StatusCode);
        var attempt = await ReadAsync<ContestAttemptResponse>(attemptResponse);

        await SeedAttemptResultAsync(attempt.AttemptId, missionIds[0]);

        var attemptsResponse = await participant.Client.GetAsync($"contests/{contest.Id}/attempts/my", Ct);
        Assert.Equal(HttpStatusCode.OK, attemptsResponse.StatusCode);
        var attempts = await ReadAsync<List<ContestAttemptDetailsResponse>>(attemptsResponse);
        Assert.Single(attempts);
        var attemptDetails = attempts[0];
        Assert.Equal(ContestAttemptStatus.Completed, attemptDetails.Status);
        Assert.Equal(1, attemptDetails.SolvedCount);
        Assert.Single(attemptDetails.MissionResults);
        Assert.Equal(missionIds[0], attemptDetails.MissionResults[0].MissionId);
        Assert.Single(attemptDetails.Submissions);
        Assert.Equal(missionIds[0], attemptDetails.Submissions[0].Solution.MissionId);
    }

    private async Task<(ContestResponse Contest, IReadOnlyList<int> MissionIds)> CreateContestAsync(
        AuthenticatedUser organizer,
        ContestScheduleType scheduleType,
        ContestVisibility visibility,
        DateTime? startsAt,
        DateTime? endsAt,
        int attemptDurationMinutes,
        int maxAttempts,
        int? groupId = null,
        IEnumerable<int>? missionIds = null,
        bool includeArticle = false)
    {
        var missionList = missionIds?.ToList() ?? new List<int>();
        if (missionList.Count == 0)
        {
            var missionId = await TestClientHelper.CreateMissionInDatabaseAsync(_fixture, organizer.UserId);
            missionList.Add(missionId);
        }

        List<int>? articleIds = null;
        if (includeArticle)
        {
            var articleId = await TestClientHelper.CreateArticleInDatabaseAsync(_fixture, organizer.UserId);
            articleIds = new List<int> { articleId };
        }

        var request = new CreateContestRequest(
            $"Contest {Guid.NewGuid():N}",
            "Integration test contest",
            scheduleType,
            visibility,
            startsAt,
            endsAt,
            attemptDurationMinutes,
            maxAttempts,
            true,
            groupId,
            missionList,
            articleIds);

        var response = await organizer.Client.PostAsJsonAsync("contests", request, Ct);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var contest = await ReadAsync<ContestResponse>(response);
        return (contest, missionList);
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        var payload = await response.Content.ReadFromJsonAsync<T>(JsonOptions, Ct);
        Assert.NotNull(payload);
        return payload!;
    }

    private async Task SeedSubmissionAsync(int contestId, int userId, int missionId)
    {
        await using var scope = _fixture.Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<LiquidDbContext>();
        var contest = await context.Contests.Include(c => c.Memberships).FirstAsync(c => c.Id == contestId, Ct);
        var user = await context.Users.FindAsync(new object?[] { userId }, Ct) ?? throw new InvalidOperationException("User not found");
        var mission = await context.Missions.FindAsync(new object?[] { missionId }, Ct) ?? throw new InvalidOperationException("Mission not found");

        var solution = new DbSolution
        {
            Mission = mission,
            Language = "csharp",
            LanguageVersion = "12",
            SourceCode = "class Solution {}",
            Status = "Accepted",
            TestingState = TesterState.Done,
            TestingErrorCode = TesterErrorCode.None,
            TestingMessage = null,
            CurrentTest = 0,
            AmountOfTests = 0,
            Time = DateTime.UtcNow
        };

        var submission = new DbUserSubmission
        {
            User = user,
            Solution = solution,
            ContestId = contestId,
            Contest = contest,
            ContestAttemptId = null,
            ContestAttempt = null,
            SourceType = SubmissionSourceType.Contest
        };

        await context.Solutions.AddAsync(solution, Ct);
        await context.UserSubmits.AddAsync(submission, Ct);
        await context.SaveChangesAsync(Ct);
    }

    private async Task SeedAttemptResultAsync(int attemptId, int missionId)
    {
        await using var scope = _fixture.Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<LiquidDbContext>();
        var attempt = await context.ContestAttempts
            .Include(a => a.Contest)
            .Include(a => a.User)
            .Include(a => a.MissionResults)
            .Include(a => a.Submissions)
            .FirstAsync(a => a.Id == attemptId, Ct);
        var mission = await context.Missions.FindAsync(new object?[] { missionId }, Ct) ?? throw new InvalidOperationException("Mission not found");

        attempt.Status = ContestAttemptStatus.Completed;
        attempt.FinishedBy = ContestAttemptFinishReason.Manual;
        attempt.FinishedAt = attempt.StartedAt.AddMinutes(5);
        attempt.TotalScore = 100;
        attempt.SolvedCount = 1;

        var result = attempt.MissionResults.FirstOrDefault(r => r.MissionId == mission.Id);
        if (result == null)
        {
            result = new DbContestAttemptMissionResult
            {
                ContestAttempt = attempt,
                ContestAttemptId = attempt.Id,
                Mission = mission,
                MissionId = mission.Id
            };
            attempt.MissionResults.Add(result);
            await context.ContestAttemptMissionResults.AddAsync(result, Ct);
        }

        result.SolvedAt = attempt.FinishedAt;
        result.SubmissionCount = 2;
        result.HighestScore = 100;
        result.Penalty = 0;
        result.FirstAcceptedAt = attempt.FinishedAt;
        result.LastSubmissionAt = attempt.FinishedAt;
        result.BestSubmissionId = null;

        if (!attempt.Submissions.Any())
        {
            var solution = new DbSolution
            {
                Mission = mission,
                Language = "csharp",
                LanguageVersion = "12",
                SourceCode = "class Solution {}",
                Status = "Accepted",
                TestingState = TesterState.Done,
                TestingErrorCode = TesterErrorCode.None,
                TestingMessage = null,
                CurrentTest = 0,
                AmountOfTests = 0,
                Time = attempt.FinishedAt ?? DateTime.UtcNow
            };

            var submission = new DbUserSubmission
            {
                User = attempt.User,
                Solution = solution,
                ContestId = attempt.ContestId,
                Contest = attempt.Contest,
                ContestAttemptId = attempt.Id,
                ContestAttempt = attempt,
                SourceType = SubmissionSourceType.Contest
            };

            attempt.Submissions.Add(submission);
            await context.Solutions.AddAsync(solution, Ct);
            await context.UserSubmits.AddAsync(submission, Ct);
        }

        await context.SaveChangesAsync(Ct);
    }
}
