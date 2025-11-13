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

    [Fact]
    public async Task GetMessages_LongPolling_ReturnsNewMessageWhenArrives()
    {
        // Arrange: создаём группу и двух участников
        var (adminClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "chat_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(adminClient, TestDataGenerator.UniqueGroupName("Chat Group"));
        var (memberClient, _, _, _) = await TestClientHelper.CreateGroupMemberAsync(_fixture, adminClient, groupId, "chat_member_polling");

        // Отправляем первое сообщение, чтобы получить его ID
        var firstMessage = await TestClientHelper.SendGroupChatMessageAsync(adminClient, groupId, "Initial message");
        Assert.NotNull(firstMessage);

        // Act: запускаем long polling запрос в отдельной задаче
        var longPollingTask = Task.Run(async () =>
        {
            var response = await memberClient.GetAsync(
                $"groups/{groupId}/chat?limit=10&afterMessageId={firstMessage.Id}&timeoutSeconds=10",
                TestContext.Current.CancellationToken);
            
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var messages = await response.Content.ReadFromJsonAsync<List<GroupChatMessageResponse>>(
                JsonOptions, 
                TestContext.Current.CancellationToken);
            
            return messages;
        });

        // Ждём немного, чтобы убедиться что long polling запрос начался
        await Task.Delay(1000, TestContext.Current.CancellationToken);

        // Отправляем новое сообщение, пока long polling ждёт
        var newMessage = await TestClientHelper.SendGroupChatMessageAsync(adminClient, groupId, "New message during polling");
        Assert.NotNull(newMessage);

        // Assert: long polling должен получить новое сообщение
        var receivedMessages = await longPollingTask;
        Assert.NotNull(receivedMessages);
        Assert.Single(receivedMessages!);
        Assert.Equal(newMessage.Id, receivedMessages[0].Id);
        Assert.Equal("New message during polling", receivedMessages[0].Content);
    }

    [Fact]
    public async Task GetMessages_LongPolling_ReturnsEmptyListAfterTimeout()
    {
        // Arrange: создаём группу и участника
        var (adminClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "chat_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(adminClient, TestDataGenerator.UniqueGroupName("Chat Group"));
        var (memberClient, _, _, _) = await TestClientHelper.CreateGroupMemberAsync(_fixture, adminClient, groupId, "chat_member_timeout");

        // Отправляем сообщение, чтобы получить его ID
        var message = await TestClientHelper.SendGroupChatMessageAsync(adminClient, groupId, "Only message");
        Assert.NotNull(message);

        // Act: запускаем long polling с коротким таймаутом и НЕ отправляем новых сообщений
        var startTime = DateTime.UtcNow;
        var response = await memberClient.GetAsync(
            $"groups/{groupId}/chat?limit=10&afterMessageId={message.Id}&timeoutSeconds=2",
            TestContext.Current.CancellationToken);
        var elapsed = DateTime.UtcNow - startTime;

        // Assert: должен вернуть пустой список после истечения таймаута
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var messages = await response.Content.ReadFromJsonAsync<List<GroupChatMessageResponse>>(
            JsonOptions, 
            TestContext.Current.CancellationToken);
        
        Assert.NotNull(messages);
        Assert.Empty(messages!);
        
        // Проверяем, что запрос действительно ждал около 2 секунд
        Assert.True(elapsed.TotalSeconds >= 1.5, $"Expected at least 1.5s wait, but got {elapsed.TotalSeconds}s");
        Assert.True(elapsed.TotalSeconds <= 3, $"Expected at most 3s wait, but got {elapsed.TotalSeconds}s");
    }

    [Fact]
    public async Task GetMessages_WithoutFilters_ReturnsLatestMessages()
    {
        // Arrange: создаём группу и отправляем много сообщений
        var (adminClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "chat_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(adminClient, TestDataGenerator.UniqueGroupName("Chat Group"));

        // Отправляем 10 сообщений
        var sentMessages = new List<GroupChatMessageResponse>();
        for (var i = 1; i <= 10; i++)
        {
            var msg = await TestClientHelper.SendGroupChatMessageAsync(adminClient, groupId, $"Message {i}");
            sentMessages.Add(msg);
        }

        var (memberClient, _, _, _) = await TestClientHelper.CreateGroupMemberAsync(_fixture, adminClient, groupId, "chat_member_latest");

        // Act: запрашиваем последние 5 сообщений БЕЗ фильтров
        var response = await memberClient.GetAsync($"groups/{groupId}/chat?limit=5&timeoutSeconds=0", TestContext.Current.CancellationToken);

        // Assert: должны получить последние 5 сообщений (6, 7, 8, 9, 10)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var messages = await response.Content.ReadFromJsonAsync<List<GroupChatMessageResponse>>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(messages);
        Assert.Equal(5, messages!.Count);

        // Проверяем, что это именно последние 5 сообщений в правильном порядке
        var expectedMessages = sentMessages.Skip(5).Take(5).ToList();
        for (var i = 0; i < 5; i++)
        {
            Assert.Equal(expectedMessages[i].Id, messages[i].Id);
            Assert.Equal(expectedMessages[i].Content, messages[i].Content);
        }

        // Проверяем, что сообщения идут в порядке от старых к новым
        Assert.Equal("Message 6", messages[0].Content);
        Assert.Equal("Message 10", messages[4].Content);
    }
}
