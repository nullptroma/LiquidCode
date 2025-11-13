using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LiquidCode.Api.Authentication.Requests;
using LiquidCode.Api.Authentication.Responses;
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

    #region Helpers

    private async Task<(HttpClient Client, string Username, int UserId, string Jwt)> CreateAuthenticatedClientAsync(string prefix)
    {
        var client = _fixture.CreateClient();
        var username = TestDataGenerator.UniqueUsername(prefix);
        var password = TestDataGenerator.ValidPassword();
        var registerRequest = new RegisterRequest(username, TestDataGenerator.EmailFor(username), password);

        var registerResponse = await client.PostAsJsonAsync("authentication/register", registerRequest, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var tokens = await registerResponse.Content.ReadFromJsonAsync<AuthTokensResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(tokens);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.Jwt);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(tokens.Jwt);
        var userIdClaim = jwtToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
        var userId = int.Parse(userIdClaim.Value);

        return (client, username, userId, tokens.Jwt);
    }

    private async Task<(int GroupId, string Name)> CreateGroupAsync(HttpClient client, string? description = null)
    {
        var groupName = TestDataGenerator.UniqueGroupName("Feed Group");
        var request = new CreateGroupRequest(groupName, description);

        var response = await client.PostAsJsonAsync("groups", request, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var group = await response.Content.ReadFromJsonAsync<GroupResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(group);
        return (group!.Id, group.Name);
    }

    private async Task<(HttpClient Client, string Username, int UserId, string Jwt)> CreateMemberClientAsync(HttpClient adminClient, int groupId, string prefix)
    {
        var (client, username, userId, jwt) = await CreateAuthenticatedClientAsync(prefix);

        var linkResponse = await adminClient.GetAsync($"groups/{groupId}/join-link", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, linkResponse.StatusCode);

        var joinLink = await linkResponse.Content.ReadFromJsonAsync<GroupJoinLinkResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(joinLink);

        var joinResponse = await client.PostAsync($"groups/join/{joinLink!.Token}", null, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, joinResponse.StatusCode);

        return (client, username, userId, jwt);
    }

    private async Task<GroupFeedPostResponse> CreateFeedPostAsync(HttpClient client, int groupId, string name, string content)
    {
        var request = new CreateGroupFeedPostRequest(name, content);
        var response = await client.PostAsJsonAsync($"groups/{groupId}/feed", request, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var post = await response.Content.ReadFromJsonAsync<GroupFeedPostResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(post);
        return post!;
    }

    #endregion

    [Fact]
    public async Task CreateFeedPost_AsAdministrator_ReturnsPost()
    {
        var (adminClient, adminUsername, adminId, _) = await CreateAuthenticatedClientAsync("feed_admin");
        var (groupId, _) = await CreateGroupAsync(adminClient);

        var request = new CreateGroupFeedPostRequest("Announcement", "Welcome to the group!");
        var response = await adminClient.PostAsJsonAsync($"groups/{groupId}/feed", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var post = await response.Content.ReadFromJsonAsync<GroupFeedPostResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(post);
        Assert.Equal(groupId, post!.GroupId);
        Assert.Equal(adminId, post.AuthorId);
        Assert.Equal(adminUsername, post.AuthorUsername);
        Assert.Equal("Announcement", post.Name);
        Assert.Equal("Welcome to the group!", post.Content);
    }

    [Fact]
    public async Task CreateFeedPost_AsMember_ReturnsNotFound()
    {
        var (adminClient, _, _, _) = await CreateAuthenticatedClientAsync("feed_owner");
        var (groupId, _) = await CreateGroupAsync(adminClient);

        var (memberClient, _, _, _) = await CreateMemberClientAsync(adminClient, groupId, "feed_member");
        var request = new CreateGroupFeedPostRequest("News", "Member update");

        var response = await memberClient.PostAsJsonAsync($"groups/{groupId}/feed", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetFeedPage_AsMember_ReturnsPosts()
    {
        var (adminClient, _, _, _) = await CreateAuthenticatedClientAsync("feed_owner");
        var (groupId, _) = await CreateGroupAsync(adminClient);

        var posts = new List<GroupFeedPostResponse>();
        for (var i = 0; i < 3; i++)
        {
            posts.Add(await CreateFeedPostAsync(adminClient, groupId, $"Post {i}", $"Content {i}"));
        }

        var (memberClient, _, _, _) = await CreateMemberClientAsync(adminClient, groupId, "feed_member2");

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
        var (adminClient, _, _, _) = await CreateAuthenticatedClientAsync("feed_owner");
        var (groupId, _) = await CreateGroupAsync(adminClient);
        await CreateFeedPostAsync(adminClient, groupId, "Post", "Content");

        var (strangerClient, _, _, _) = await CreateAuthenticatedClientAsync("feed_stranger");

        var response = await strangerClient.GetAsync($"groups/{groupId}/feed", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateFeedPost_AsAuthor_ReturnsUpdatedPost()
    {
        var (adminClient, _, _, _) = await CreateAuthenticatedClientAsync("feed_owner");
        var (groupId, _) = await CreateGroupAsync(adminClient);
        var post = await CreateFeedPostAsync(adminClient, groupId, "Initial", "Initial content");

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
        var (adminClient, _, _, _) = await CreateAuthenticatedClientAsync("feed_owner");
        var (groupId, _) = await CreateGroupAsync(adminClient);
        var post = await CreateFeedPostAsync(adminClient, groupId, "Initial", "Initial content");

        var (memberClient, _, _, _) = await CreateMemberClientAsync(adminClient, groupId, "feed_member3");
        var updateRequest = new UpdateGroupFeedPostRequest("Hack", "Hacked content");

        var response = await memberClient.PutAsJsonAsync($"groups/{groupId}/feed/{post.Id}", updateRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteFeedPost_AsAdministrator_ReturnsNoContent()
    {
        var (adminClient, _, _, _) = await CreateAuthenticatedClientAsync("feed_owner");
        var (groupId, _) = await CreateGroupAsync(adminClient);
        var post = await CreateFeedPostAsync(adminClient, groupId, "Initial", "Initial content");

        var response = await adminClient.DeleteAsync($"groups/{groupId}/feed/{post.Id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await adminClient.GetAsync($"groups/{groupId}/feed/{post.Id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetFeedPost_NonMember_ReturnsNotFound()
    {
        var (adminClient, _, _, _) = await CreateAuthenticatedClientAsync("feed_owner");
        var (groupId, _) = await CreateGroupAsync(adminClient);
        var post = await CreateFeedPostAsync(adminClient, groupId, "Initial", "Initial content");

        var (strangerClient, _, _, _) = await CreateAuthenticatedClientAsync("feed_stranger");

        var response = await strangerClient.GetAsync($"groups/{groupId}/feed/{post.Id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetFeedPage_WithInvalidPagination_ReturnsBadRequest()
    {
        var (adminClient, _, _, _) = await CreateAuthenticatedClientAsync("feed_owner");
        var (groupId, _) = await CreateGroupAsync(adminClient);

        var response = await adminClient.GetAsync($"groups/{groupId}/feed?pageSize=0&page=0", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
