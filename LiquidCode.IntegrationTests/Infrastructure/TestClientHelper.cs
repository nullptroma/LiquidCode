using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using LiquidCode.Infrastructure.Database;
using LiquidCode.Infrastructure.Database.Entities;
using LiquidCode.Api.Authentication.Requests;
using LiquidCode.Api.Authentication.Responses;
using LiquidCode.Api.Groups.Requests;
using LiquidCode.Api.Groups.Responses;

namespace LiquidCode.IntegrationTests.Infrastructure;

/// <summary>
/// Authenticated user context with client and metadata
/// </summary>
internal record AuthenticatedUser(HttpClient Client, string Username, int UserId, string Jwt);

internal static class TestClientHelper
{
    private static readonly JsonSerializerOptions JsonOptions = TestJsonOptions.Default;
    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    /// <summary>
    /// Creates and registers a new authenticated user
    /// </summary>
    public static async Task<AuthenticatedUser> CreateAuthenticatedUserAsync(
        IntegrationTestFixture fixture, 
        string prefix)
    {
        var client = fixture.CreateClient();
        var username = TestDataGenerator.UniqueUsername(prefix);
        var password = TestDataGenerator.ValidPassword();
        var registerRequest = new RegisterRequest(username, TestDataGenerator.EmailFor(username), password);

        var registerResponse = await client.PostAsJsonAsync("authentication/register", registerRequest, CancellationToken);
        
        var tokens = await EnsureSuccessAndReadAsync<AuthTokensResponse>(registerResponse);
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.Jwt);

        var userId = ExtractUserIdFromJwt(tokens.Jwt);

        return new AuthenticatedUser(client, username, userId, tokens.Jwt);
    }

    /// <summary>
    /// Creates a group with a specific name
    /// </summary>
    public static async Task<(int GroupId, string Name)> CreateGroupAsync(
        HttpClient client, 
        string name,
        string? description = null)
    {
        var request = new CreateGroupRequest(name, description);

        var response = await client.PostAsJsonAsync("groups", request, CancellationToken);
        var group = await EnsureSuccessAndReadAsync<GroupResponse>(response);
        
        return (group.Id, group.Name);
    }

    /// <summary>
    /// Creates a group with an auto-generated unique name
    /// </summary>
    public static Task<(int GroupId, string Name)> CreateGroupWithUniqueNameAsync(
        HttpClient client,
        string? description = null)
    {
        var groupName = TestDataGenerator.UniqueGroupName("Group");
        return CreateGroupAsync(client, groupName, description);
    }

    /// <summary>
    /// Creates a new user and adds them as a member to the specified group
    /// </summary>
    public static async Task<AuthenticatedUser> CreateGroupMemberAsync(
        IntegrationTestFixture fixture, 
        HttpClient adminClient, 
        int groupId, 
        string usernamePrefix)
    {
        var user = await CreateAuthenticatedUserAsync(fixture, usernamePrefix);

        var joinLink = await GetGroupJoinLinkAsync(adminClient, groupId);
        await JoinGroupByTokenAsync(user.Client, joinLink.Token);

        return user;
    }

    /// <summary>
    /// Gets a join link for a group
    /// </summary>
    public static async Task<GroupJoinLinkResponse> GetGroupJoinLinkAsync(HttpClient client, int groupId)
    {
        var response = await client.GetAsync($"groups/{groupId}/join-link", CancellationToken);
        return await EnsureSuccessAndReadAsync<GroupJoinLinkResponse>(response);
    }

    /// <summary>
    /// Joins a group using a token
    /// </summary>
    public static async Task<GroupResponse> JoinGroupByTokenAsync(HttpClient client, string token)
    {
        var response = await client.PostAsync($"groups/join/{token}", null, CancellationToken);
        return await EnsureSuccessAndReadAsync<GroupResponse>(response);
    }

    /// <summary>
    /// Sends a chat message to a group
    /// </summary>
    public static async Task<GroupChatMessageResponse> SendGroupChatMessageAsync(
        HttpClient client, 
        int groupId, 
        string content)
    {
        var request = new CreateGroupChatMessageRequest(content);
        var response = await client.PostAsJsonAsync($"groups/{groupId}/chat", request, CancellationToken);
        return await EnsureSuccessAndReadAsync<GroupChatMessageResponse>(response);
    }

    /// <summary>
    /// Creates a feed post in a group
    /// </summary>
    public static async Task<GroupFeedPostResponse> CreateGroupFeedPostAsync(
        HttpClient client, 
        int groupId, 
        string name, 
        string content)
    {
        var request = new CreateGroupFeedPostRequest(name, content);
        var response = await client.PostAsJsonAsync($"groups/{groupId}/feed", request, CancellationToken);
        return await EnsureSuccessAndReadAsync<GroupFeedPostResponse>(response);
    }

    /// <summary>
    /// Inserts a mission directly into the test database for the specified author
    /// </summary>
    public static async Task<int> CreateMissionInDatabaseAsync(IntegrationTestFixture fixture, int authorId, string? name = null)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LiquidDbContext>();
        var user = await context.Users.FindAsync(authorId);
        if (user is null)
            throw new InvalidOperationException($"User not found: {authorId}");

        var mission = new DbMission
        {
            Author = user,
            Name = name ?? TestDataGenerator.UniqueMissionName(),
            S3Key = "test-key",
            Difficulty = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await context.Missions.AddAsync(mission, CancellationToken);
        await context.SaveChangesAsync(CancellationToken);

        return mission.Id;
    }

    /// <summary>
    /// Inserts an article directly into the database for the specified author
    /// </summary>
    public static async Task<int> CreateArticleInDatabaseAsync(IntegrationTestFixture fixture, int authorId, string? name = null, string? content = null)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LiquidDbContext>();
        var author = await context.Users.FindAsync(authorId);
        if (author is null)
            throw new InvalidOperationException($"User not found: {authorId}");

        var articleName = name ?? TestDataGenerator.UniqueArticleName();
        var articleContent = content ?? TestDataGenerator.ArticleContent(articleName);

        var article = new DbArticle
        {
            Author = author,
            Name = articleName,
            Content = articleContent,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await context.Articles.AddAsync(article, CancellationToken);
        await context.SaveChangesAsync(CancellationToken);

        return article.Id;
    }

    // Private helper methods

    private static async Task<T> EnsureSuccessAndReadAsync<T>(HttpResponseMessage response) where T : class
    {
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(CancellationToken);
            throw new InvalidOperationException(
                $"Request failed with status {response.StatusCode}. " +
                $"Response: {content}");
        }

        var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions, CancellationToken);
        
        if (result is null)
        {
            throw new InvalidOperationException($"Failed to deserialize response to {typeof(T).Name}");
        }

        return result;
    }

    private static int ExtractUserIdFromJwt(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(jwt);
        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        
        if (userIdClaim is null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            throw new InvalidOperationException("Failed to extract user ID from JWT");
        }

        return userId;
    }

}
