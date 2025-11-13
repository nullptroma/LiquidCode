using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using LiquidCode.Api.Groups.Requests;
using LiquidCode.Api.Groups.Responses;
using LiquidCode.IntegrationTests.Infrastructure;

namespace LiquidCode.IntegrationTests.Tests;

[Collection(IntegrationTestCollection.Name)]
public class GroupFeedControllerTests
{
    private readonly IntegrationTestFixture _fixture;
    private static readonly JsonSerializerOptions JsonOptions = TestJsonOptions.Default;

    public GroupFeedControllerTests(IntegrationTestFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task CreateFeedPost_AsAdministrator_ReturnsPost()
    {
        var (adminClient, adminUsername, adminId, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "feed_admin");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(adminClient, TestDataGenerator.UniqueGroupName("Feed Group"));

        var post = await TestClientHelper.CreateGroupFeedPostAsync(adminClient, groupId, "Announcement", "Welcome to the group!");
        Assert.Equal(groupId, post!.GroupId);
        Assert.Equal(adminId, post.AuthorId);
        Assert.Equal(adminUsername, post.AuthorUsername);
        Assert.Equal("Announcement", post.Name);
        Assert.Equal("Welcome to the group!", post.Content);
    }

    [Fact]
    public async Task CreateFeedPost_AsMember_ReturnsNotFound()
    {
        var (adminClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "feed_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(adminClient, TestDataGenerator.UniqueGroupName("Feed Group"));

        var (memberClient, _, _, _) = await TestClientHelper.CreateGroupMemberAsync(_fixture, adminClient, groupId, "feed_member");
        var request = new CreateGroupFeedPostRequest("News", "Member update");

        var response = await memberClient.PostAsJsonAsync($"groups/{groupId}/feed", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetFeedPage_AsMember_ReturnsPosts()
    {
        var (adminClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "feed_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(adminClient, TestDataGenerator.UniqueGroupName("Feed Group"));

        var posts = new List<GroupFeedPostResponse>();
        for (var i = 0; i < 3; i++)
        {
            posts.Add(await TestClientHelper.CreateGroupFeedPostAsync(adminClient, groupId, $"Post {i}", $"Content {i}"));
        }

        var (memberClient, _, _, _) = await TestClientHelper.CreateGroupMemberAsync(_fixture, adminClient, groupId, "feed_member2");

        var response = await memberClient.GetAsync($"groups/{groupId}/feed?pageSize=10&page=0", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var page = await response.Content.ReadFromJsonAsync<GroupFeedPageResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(page);
        Assert.False(page!.HasNext);
        Assert.Equal(3, page.Items.Count);
        var returnedIds = page.Items.Select(p => p.Id).ToHashSet();
        foreach (var post in posts)
        {
            Assert.Contains(post.Id, returnedIds);
        }
    }

    [Fact]
    public async Task GetFeedPage_NonMember_ReturnsNotFound()
    {
        var (adminClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "feed_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(adminClient, TestDataGenerator.UniqueGroupName("Feed Group"));
        await TestClientHelper.CreateGroupFeedPostAsync(adminClient, groupId, "Post", "Content");

        var (strangerClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "feed_stranger");

        var response = await strangerClient.GetAsync($"groups/{groupId}/feed", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateFeedPost_AsAuthor_ReturnsUpdatedPost()
    {
        var (adminClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "feed_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(adminClient, TestDataGenerator.UniqueGroupName("Feed Group"));
        var post = await TestClientHelper.CreateGroupFeedPostAsync(adminClient, groupId, "Initial", "Initial content");

        var updateRequest = new UpdateGroupFeedPostRequest("Updated", "Updated content");
        var response = await adminClient.PutAsJsonAsync($"groups/{groupId}/feed/{post.Id}", updateRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<GroupFeedPostResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(updated);
        Assert.Equal("Updated", updated!.Name);
        Assert.Equal("Updated content", updated.Content);
    }

    [Fact]
    public async Task UpdateFeedPost_AsDifferentUser_ReturnsNotFound()
    {
        var (adminClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "feed_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(adminClient, TestDataGenerator.UniqueGroupName("Feed Group"));
        var post = await TestClientHelper.CreateGroupFeedPostAsync(adminClient, groupId, "Initial", "Initial content");

        var (memberClient, _, _, _) = await TestClientHelper.CreateGroupMemberAsync(_fixture, adminClient, groupId, "feed_member3");
        var updateRequest = new UpdateGroupFeedPostRequest("Hack", "Hacked content");

        var response = await memberClient.PutAsJsonAsync($"groups/{groupId}/feed/{post.Id}", updateRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteFeedPost_AsAdministrator_ReturnsNoContent()
    {
        var (adminClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "feed_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(adminClient, TestDataGenerator.UniqueGroupName("Feed Group"));
        var post = await TestClientHelper.CreateGroupFeedPostAsync(adminClient, groupId, "Initial", "Initial content");

        var response = await adminClient.DeleteAsync($"groups/{groupId}/feed/{post.Id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await adminClient.GetAsync($"groups/{groupId}/feed/{post.Id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetFeedPost_NonMember_ReturnsNotFound()
    {
        var (adminClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "feed_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(adminClient, TestDataGenerator.UniqueGroupName("Feed Group"));
        var post = await TestClientHelper.CreateGroupFeedPostAsync(adminClient, groupId, "Initial", "Initial content");

        var (strangerClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "feed_stranger");

        var response = await strangerClient.GetAsync($"groups/{groupId}/feed/{post.Id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetFeedPage_WithInvalidPagination_ReturnsBadRequest()
    {
        var (adminClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "feed_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(adminClient, TestDataGenerator.UniqueGroupName("Feed Group"));

        var response = await adminClient.GetAsync($"groups/{groupId}/feed?pageSize=0&page=0", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
