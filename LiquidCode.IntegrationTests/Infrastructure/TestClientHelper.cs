using System.Linq;
using System.Net;
using System.Net.Http;
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
using Xunit;

namespace LiquidCode.IntegrationTests.Infrastructure;

internal static class TestClientHelper
{
    private static readonly JsonSerializerOptions JsonOptions = TestJsonOptions.Default;

    public static async Task<(HttpClient Client, string Username, int UserId, string Jwt)> CreateAuthenticatedClientAsync(IntegrationTestFixture fixture, string prefix)
    {
        var client = fixture.CreateClient();
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

    public static async Task<(int GroupId, string Name)> CreateGroupAsync(HttpClient client, string? name = null, string? description = null)
    {
        var groupName = name ?? TestDataGenerator.UniqueGroupName();
        var request = new CreateGroupRequest(groupName, description);

        var response = await client.PostAsJsonAsync("groups", request, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var group = await response.Content.ReadFromJsonAsync<GroupResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(group);
        return (group!.Id, group.Name);
    }

    public static async Task<(HttpClient Client, string Username, int UserId, string Jwt)> CreateMemberClientAsync(IntegrationTestFixture fixture, HttpClient adminClient, int groupId, string prefix)
    {
        var (client, username, userId, jwt) = await CreateAuthenticatedClientAsync(fixture, prefix);

        var linkResponse = await adminClient.GetAsync($"groups/{groupId}/join-link", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, linkResponse.StatusCode);

        var joinLink = await linkResponse.Content.ReadFromJsonAsync<GroupJoinLinkResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(joinLink);

        var joinResponse = await client.PostAsync($"groups/join/{joinLink!.Token}", null, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, joinResponse.StatusCode);

        return (client, username, userId, jwt);
    }

    public static async Task<GroupChatMessageResponse> SendGroupChatMessageAsync(HttpClient client, int groupId, string content)
    {
        var request = new CreateGroupChatMessageRequest(content);
        var response = await client.PostAsJsonAsync($"groups/{groupId}/chat", request, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var message = await response.Content.ReadFromJsonAsync<GroupChatMessageResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(message);
        return message!;
    }

    public static async Task<GroupFeedPostResponse> CreateGroupFeedPostAsync(HttpClient client, int groupId, string name, string content)
    {
        var request = new CreateGroupFeedPostRequest(name, content);
        var response = await client.PostAsJsonAsync($"groups/{groupId}/feed", request, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var post = await response.Content.ReadFromJsonAsync<GroupFeedPostResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(post);
        return post!;
    }
}
