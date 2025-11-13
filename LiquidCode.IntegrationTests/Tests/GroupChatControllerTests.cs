using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
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

    [Fact]
    public async Task SendMessage_AsMember_ReturnsMessage()
    {
        var (adminClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "chat_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(adminClient, TestDataGenerator.UniqueGroupName("Chat Group"));
        var (memberClient, memberUsername, memberUserId, _) = await TestClientHelper.CreateGroupMemberAsync(_fixture, adminClient, groupId, "chat_member");

        var message = await TestClientHelper.SendGroupChatMessageAsync(memberClient, groupId, "Hello everyone");
        Assert.Equal(groupId, message!.GroupId);
        Assert.Equal(memberUserId, message.AuthorId);
        Assert.Equal(memberUsername, message.AuthorUsername);
        Assert.Equal("Hello everyone", message.Content);
    }

    [Fact]
    public async Task SendMessage_NonMember_ReturnsNotFound()
    {
        var (adminClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "chat_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(adminClient, TestDataGenerator.UniqueGroupName("Chat Group"));

        var (strangerClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "chat_stranger");

        var response = await strangerClient.PostAsJsonAsync($"groups/{groupId}/chat", new CreateGroupChatMessageRequest("Hi"), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SendMessage_WithEmptyContent_ReturnsBadRequest()
    {
        var (adminClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "chat_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(adminClient, TestDataGenerator.UniqueGroupName("Chat Group"));
        var (memberClient, _, _, _) = await TestClientHelper.CreateGroupMemberAsync(_fixture, adminClient, groupId, "chat_member2");

        var response = await memberClient.PostAsJsonAsync($"groups/{groupId}/chat", new CreateGroupChatMessageRequest(string.Empty), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetMessages_AsMember_ReturnsMessages()
    {
        var (adminClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "chat_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(adminClient, TestDataGenerator.UniqueGroupName("Chat Group"));

        var sentMessages = new List<GroupChatMessageResponse>();
        for (var i = 0; i < 3; i++)
        {
            sentMessages.Add(await TestClientHelper.SendGroupChatMessageAsync(adminClient, groupId, $"Message {i}"));
        }

        var (memberClient, _, _, _) = await TestClientHelper.CreateGroupMemberAsync(_fixture, adminClient, groupId, "chat_member3");

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
        var (adminClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "chat_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(adminClient, TestDataGenerator.UniqueGroupName("Chat Group"));
        await TestClientHelper.SendGroupChatMessageAsync(adminClient, groupId, "Hello");

        var (strangerClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "chat_stranger");

        var response = await strangerClient.GetAsync($"groups/{groupId}/chat?limit=10", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetMessages_WithNonPositiveLimit_ReturnsBadRequest()
    {
        var (adminClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "chat_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(adminClient, TestDataGenerator.UniqueGroupName("Chat Group"));
        var (memberClient, _, _, _) = await TestClientHelper.CreateGroupMemberAsync(_fixture, adminClient, groupId, "chat_member4");

        var response = await memberClient.GetAsync($"groups/{groupId}/chat?limit=0", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
