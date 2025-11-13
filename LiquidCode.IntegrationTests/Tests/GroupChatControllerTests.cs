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
public class GroupChatControllerTests
{
    private readonly IntegrationTestFixture _fixture;
    private static readonly JsonSerializerOptions JsonOptions = TestJsonOptions.Default;

    public GroupChatControllerTests(IntegrationTestFixture fixture) => _fixture = fixture;

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
        var groupName = TestDataGenerator.UniqueGroupName("Chat Group");
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

    private async Task<GroupChatMessageResponse> SendMessageAsync(HttpClient client, int groupId, string content)
    {
        var request = new CreateGroupChatMessageRequest(content);
        var response = await client.PostAsJsonAsync($"groups/{groupId}/chat", request, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var message = await response.Content.ReadFromJsonAsync<GroupChatMessageResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(message);
        return message!;
    }

    #endregion

    [Fact]
    public async Task SendMessage_AsMember_ReturnsMessage()
    {
        var (adminClient, _, _, _) = await CreateAuthenticatedClientAsync("chat_owner");
        var (groupId, _) = await CreateGroupAsync(adminClient);
        var (memberClient, memberUsername, memberUserId, _) = await CreateMemberClientAsync(adminClient, groupId, "chat_member");

        var response = await memberClient.PostAsJsonAsync($"groups/{groupId}/chat", new CreateGroupChatMessageRequest("Hello everyone"), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var message = await response.Content.ReadFromJsonAsync<GroupChatMessageResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(message);
        Assert.Equal(groupId, message!.GroupId);
        Assert.Equal(memberUserId, message.AuthorId);
        Assert.Equal(memberUsername, message.AuthorUsername);
        Assert.Equal("Hello everyone", message.Content);
    }

    [Fact]
    public async Task SendMessage_NonMember_ReturnsNotFound()
    {
        var (adminClient, _, _, _) = await CreateAuthenticatedClientAsync("chat_owner");
        var (groupId, _) = await CreateGroupAsync(adminClient);

        var (strangerClient, _, _, _) = await CreateAuthenticatedClientAsync("chat_stranger");

        var response = await strangerClient.PostAsJsonAsync($"groups/{groupId}/chat", new CreateGroupChatMessageRequest("Hi"), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SendMessage_WithEmptyContent_ReturnsBadRequest()
    {
        var (adminClient, _, _, _) = await CreateAuthenticatedClientAsync("chat_owner");
        var (groupId, _) = await CreateGroupAsync(adminClient);
        var (memberClient, _, _, _) = await CreateMemberClientAsync(adminClient, groupId, "chat_member2");

        var response = await memberClient.PostAsJsonAsync($"groups/{groupId}/chat", new CreateGroupChatMessageRequest(string.Empty), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetMessages_AsMember_ReturnsMessages()
    {
        var (adminClient, _, _, _) = await CreateAuthenticatedClientAsync("chat_owner");
        var (groupId, _) = await CreateGroupAsync(adminClient);

        var sentMessages = new List<GroupChatMessageResponse>();
        for (var i = 0; i < 3; i++)
        {
            sentMessages.Add(await SendMessageAsync(adminClient, groupId, $"Message {i}"));
        }

        var (memberClient, _, _, _) = await CreateMemberClientAsync(adminClient, groupId, "chat_member3");

        var response = await memberClient.GetAsync($"groups/{groupId}/chat?limit=10", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var messages = await response.Content.ReadFromJsonAsync<List<GroupChatMessageResponse>>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(messages);
        Assert.Equal(3, messages!.Count);
        foreach (var expected in sentMessages)
        {
            Assert.Contains(messages, m => m.Id == expected.Id && m.Content == expected.Content);
        }
    }

    [Fact]
    public async Task GetMessages_NonMember_ReturnsNotFound()
    {
        var (adminClient, _, _, _) = await CreateAuthenticatedClientAsync("chat_owner");
        var (groupId, _) = await CreateGroupAsync(adminClient);
        await SendMessageAsync(adminClient, groupId, "Hello");

        var (strangerClient, _, _, _) = await CreateAuthenticatedClientAsync("chat_stranger");

        var response = await strangerClient.GetAsync($"groups/{groupId}/chat?limit=10", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetMessages_WithNonPositiveLimit_ReturnsBadRequest()
    {
        var (adminClient, _, _, _) = await CreateAuthenticatedClientAsync("chat_owner");
        var (groupId, _) = await CreateGroupAsync(adminClient);
        var (memberClient, _, _, _) = await CreateMemberClientAsync(adminClient, groupId, "chat_member4");

        var response = await memberClient.GetAsync($"groups/{groupId}/chat?limit=0", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
