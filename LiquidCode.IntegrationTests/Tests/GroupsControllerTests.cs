using System;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using LiquidCode.Api.Authentication.Requests;
using LiquidCode.Api.Authentication.Responses;
using LiquidCode.Api.Groups.Requests;
using LiquidCode.Api.Groups.Responses;
using LiquidCode.Infrastructure.Database.Entities;
using LiquidCode.IntegrationTests.Infrastructure;

namespace LiquidCode.IntegrationTests.Tests;

[Collection(IntegrationTestCollection.Name)]
public class GroupsControllerTests
{
    private readonly IntegrationTestFixture _fixture;
    private static readonly JsonSerializerOptions JsonOptions = TestJsonOptions.Default;

    public GroupsControllerTests(IntegrationTestFixture fixture) => _fixture = fixture;

    #region Helper Methods

    private async Task<(HttpClient client, string username, int userId, string jwt)> CreateAuthenticatedClient()
    {
        var client = _fixture.CreateClient();
        var username = TestDataGenerator.UniqueUsername("groups");
        var password = TestDataGenerator.ValidPassword();
        var request = new RegisterRequest(username, TestDataGenerator.EmailFor(username), password);

        var response = await client.PostAsJsonAsync("authentication/register", request, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var tokens = await response.Content.ReadFromJsonAsync<AuthTokensResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(tokens);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.Jwt);

        // Parse JWT to get userId (JWT payload contains userId as ClaimTypes.NameIdentifier claim)
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(tokens!.Jwt);
        var userIdClaim = jwtToken.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
        var userId = int.Parse(userIdClaim.Value);

        return (client, username, userId, tokens.Jwt);
    }

    private async Task<int> CreateGroup(HttpClient client, string name, string? description = null)
    {
        var request = new CreateGroupRequest(name, description);
        var response = await client.PostAsJsonAsync("groups", request, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var groupResponse = await response.Content.ReadFromJsonAsync<GroupResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(groupResponse);
        return groupResponse!.Id;
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidRequest_ReturnsGroup()
    {
        var (client, username, _, _) = await CreateAuthenticatedClient();
        var groupName = TestDataGenerator.UniqueGroupName();
        var description = "Test Description";
        var request = new CreateGroupRequest(groupName, description);

        var response = await client.PostAsJsonAsync("groups", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var group = await response.Content.ReadFromJsonAsync<GroupResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(group);
        Assert.Equal(groupName, group!.Name);
        Assert.Equal(description, group.Description);
        Assert.NotEmpty(group.Members);
        
        var creator = group.Members.First();
        Assert.Equal(username, creator.Username);
        Assert.True(creator.Role.HasFlag(GroupMembershipRole.Creator));
    }

    [Fact]
    public async Task Create_WithValidRequestNoDescription_ReturnsGroup()
    {
        var (client, _, _, _) = await CreateAuthenticatedClient();
        var groupName = TestDataGenerator.UniqueGroupName();
        var request = new CreateGroupRequest(groupName, null);

        var response = await client.PostAsJsonAsync("groups", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var group = await response.Content.ReadFromJsonAsync<GroupResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(group);
        Assert.Equal(groupName, group!.Name);
        Assert.Null(group.Description);
    }

    [Fact]
    public async Task Create_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var client = _fixture.CreateClient();
        var request = new CreateGroupRequest(TestDataGenerator.UniqueGroupName(), "Description");

        var response = await client.PostAsJsonAsync("groups", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithInvalidName_ReturnsBadRequest()
    {
        var (client, _, _, _) = await CreateAuthenticatedClient();
        var request = new CreateGroupRequest("AB", "Description"); // Too short (min 3)

        var response = await client.PostAsJsonAsync("groups", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithTooLongName_ReturnsBadRequest()
    {
        var (client, _, _, _) = await CreateAuthenticatedClient();
        var longName = new string('A', 129); // Max 128
        var request = new CreateGroupRequest(longName, "Description");

        var response = await client.PostAsJsonAsync("groups", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Get Tests

    [Fact]
    public async Task Get_ExistingGroup_ReturnsGroup()
    {
        var (client, _, _, _) = await CreateAuthenticatedClient();
        var groupName = TestDataGenerator.UniqueGroupName();
        var groupId = await CreateGroup(client, groupName);

        var response = await client.GetAsync($"groups/{groupId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var group = await response.Content.ReadFromJsonAsync<GroupResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(group);
        Assert.Equal(groupId, group!.Id);
        Assert.Equal(groupName, group.Name);
    }

    [Fact]
    public async Task Get_ExistingGroupWithoutAuthentication_ReturnsGroup()
    {
        var (authClient, _, _, _) = await CreateAuthenticatedClient();
        var groupId = await CreateGroup(authClient, TestDataGenerator.UniqueGroupName());

        using var unauthClient = _fixture.CreateClient();
        var response = await unauthClient.GetAsync($"groups/{groupId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_NonExistingGroup_ReturnsNotFound()
    {
        var (client, _, _, _) = await CreateAuthenticatedClient();
        var nonExistingId = 999999;

        var response = await client.GetAsync($"groups/{nonExistingId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidRequest_ReturnsUpdatedGroup()
    {
        var (client, _, _, _) = await CreateAuthenticatedClient();
        var groupId = await CreateGroup(client, $"Old Name {Guid.NewGuid():N}", "Old Description");

        var newName = $"New Name {Guid.NewGuid():N}";
        var newDescription = "New Description";
        var updateRequest = new UpdateGroupRequest(newName, newDescription);

        var response = await client.PutAsJsonAsync($"groups/{groupId}", updateRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var group = await response.Content.ReadFromJsonAsync<GroupResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(group);
        Assert.Equal(newName, group!.Name);
        Assert.Equal(newDescription, group.Description);
    }

    [Fact]
    public async Task Update_OnlyName_UpdatesOnlyName()
    {
        var (client, _, _, _) = await CreateAuthenticatedClient();
        var oldDescription = "Old Description";
        var groupId = await CreateGroup(client, $"Old Name {Guid.NewGuid():N}", oldDescription);

        var newName = $"New Name {Guid.NewGuid():N}";
        var updateRequest = new UpdateGroupRequest(newName, null);

        var response = await client.PutAsJsonAsync($"groups/{groupId}", updateRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var group = await response.Content.ReadFromJsonAsync<GroupResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(group);
        Assert.Equal(newName, group!.Name);
        Assert.Equal(oldDescription, group.Description);
    }

    [Fact]
    public async Task Update_OnlyDescription_UpdatesOnlyDescription()
    {
        var (client, _, _, _) = await CreateAuthenticatedClient();
        var oldName = $"Old Name {Guid.NewGuid():N}";
        var groupId = await CreateGroup(client, oldName, "Old Description");

        var newDescription = "New Description";
        var updateRequest = new UpdateGroupRequest(null, newDescription);

        var response = await client.PutAsJsonAsync($"groups/{groupId}", updateRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var group = await response.Content.ReadFromJsonAsync<GroupResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(group);
        Assert.Equal(oldName, group!.Name);
        Assert.Equal(newDescription, group.Description);
    }

    [Fact]
    public async Task Update_NonExistingGroup_ReturnsNotFound()
    {
        var (client, _, _, _) = await CreateAuthenticatedClient();
        var nonExistingId = 999999;
        var updateRequest = new UpdateGroupRequest("New Name", "New Description");

        var response = await client.PutAsJsonAsync($"groups/{nonExistingId}", updateRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_OtherUsersGroup_ReturnsNotFound()
    {
        var (client1, _, _, _) = await CreateAuthenticatedClient();
        var groupId = await CreateGroup(client1, TestDataGenerator.UniqueGroupName());

        var (client2, _, _, _) = await CreateAuthenticatedClient();
        var updateRequest = new UpdateGroupRequest("Hacked Name", null);

        var response = await client2.PutAsJsonAsync($"groups/{groupId}", updateRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithoutAuthentication_ReturnsUnauthorized()
    {
        var (authClient, _, _, _) = await CreateAuthenticatedClient();
        var groupId = await CreateGroup(authClient, TestDataGenerator.UniqueGroupName());

        using var unauthClient = _fixture.CreateClient();
        var updateRequest = new UpdateGroupRequest("New Name", null);

        var response = await unauthClient.PutAsJsonAsync($"groups/{groupId}", updateRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_ExistingGroup_ReturnsNoContent()
    {
        var (client, _, _, _) = await CreateAuthenticatedClient();
        var groupId = await CreateGroup(client, TestDataGenerator.UniqueGroupName());

        var response = await client.DeleteAsync($"groups/{groupId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify group is deleted
        var getResponse = await client.GetAsync($"groups/{groupId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_NonExistingGroup_ReturnsNotFound()
    {
        var (client, _, _, _) = await CreateAuthenticatedClient();
        var nonExistingId = 999999;

        var response = await client.DeleteAsync($"groups/{nonExistingId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_OtherUsersGroup_ReturnsNotFound()
    {
        var (client1, _, _, _) = await CreateAuthenticatedClient();
        var groupId = await CreateGroup(client1, TestDataGenerator.UniqueGroupName());

        var (client2, _, _, _) = await CreateAuthenticatedClient();

        var response = await client2.DeleteAsync($"groups/{groupId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithoutAuthentication_ReturnsUnauthorized()
    {
        var (authClient, _, _, _) = await CreateAuthenticatedClient();
        var groupId = await CreateGroup(authClient, TestDataGenerator.UniqueGroupName());

        using var unauthClient = _fixture.CreateClient();

        var response = await unauthClient.DeleteAsync($"groups/{groupId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region GetMyGroups Tests

    [Fact]
    public async Task GetMyGroups_WithMultipleGroups_ReturnsAllGroups()
    {
        var (client, _, _, _) = await CreateAuthenticatedClient();
        
        var groupId1 = await CreateGroup(client, $"Group 1 {Guid.NewGuid():N}");
        var groupId2 = await CreateGroup(client, $"Group 2 {Guid.NewGuid():N}");
        var groupId3 = await CreateGroup(client, $"Group 3 {Guid.NewGuid():N}");

        var response = await client.GetAsync("groups/my", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var page = await response.Content.ReadFromJsonAsync<GroupsPageResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(page);
        
        var groups = page!.Groups.ToList();
        Assert.Contains(groups, g => g.Id == groupId1);
        Assert.Contains(groups, g => g.Id == groupId2);
        Assert.Contains(groups, g => g.Id == groupId3);
    }

    [Fact]
    public async Task GetMyGroups_WithNoGroups_ReturnsEmptyList()
    {
        var (client, _, _, _) = await CreateAuthenticatedClient();

        var response = await client.GetAsync("groups/my", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var page = await response.Content.ReadFromJsonAsync<GroupsPageResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(page);
        Assert.Empty(page!.Groups);
    }

    [Fact]
    public async Task GetMyGroups_WithPagination_ReturnsCorrectPage()
    {
        var (client, _, _, _) = await CreateAuthenticatedClient();
        
        // Create 15 groups
        for (int i = 0; i < 15; i++)
        {
            await CreateGroup(client, $"Group {i} {Guid.NewGuid():N}");
        }

        // Get first page (pageSize=10, page=0)
        var response1 = await client.GetAsync("groups/my?pageSize=10&page=0", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        var page1 = await response1.Content.ReadFromJsonAsync<GroupsPageResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(page1);
        Assert.Equal(10, page1!.Groups.Count());
        Assert.True(page1.HasNextPage);

        // Get second page
        var response2 = await client.GetAsync("groups/my?pageSize=10&page=1", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        var page2 = await response2.Content.ReadFromJsonAsync<GroupsPageResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(page2);
        Assert.Equal(5, page2!.Groups.Count());
        Assert.False(page2.HasNextPage);
    }

    [Fact]
    public async Task GetMyGroups_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var client = _fixture.CreateClient();

        var response = await client.GetAsync("groups/my", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Member Management Tests

    [Fact]
    public async Task UpdateMemberRole_NonExistentMember_ReturnsNotFound()
    {
        var (client1, _, _, _) = await CreateAuthenticatedClient();
        var groupId = await CreateGroup(client1, TestDataGenerator.UniqueGroupName());

        // Create second user
        var (client2, _, userId2, _) = await CreateAuthenticatedClient();

        // Try to update non-existent member
        var membershipRequest = new GroupMembershipRequest(userId2, GroupMembershipRole.Member);
        var response = await client1.PostAsJsonAsync($"groups/{groupId}/members", membershipRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMemberRole_UpdateExistingMember_ReturnsNoContent()
    {
        var (client1, _, _, _) = await CreateAuthenticatedClient();
        var groupId = await CreateGroup(client1, TestDataGenerator.UniqueGroupName());

        // Create second user and join via link
        var (client2, _, userId2, _) = await CreateAuthenticatedClient();
        
        // Get join link
        var linkResponse = await client1.GetAsync($"groups/{groupId}/join-link", TestContext.Current.CancellationToken);
        var joinLink = await linkResponse.Content.ReadFromJsonAsync<GroupJoinLinkResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(joinLink);
        
        // Join group using token
        await client2.PostAsync($"groups/join/{joinLink!.Token}", null, TestContext.Current.CancellationToken);

        // Promote to Administrator
        var membershipRequest = new GroupMembershipRequest(userId2, GroupMembershipRole.Administrator);
        var response = await client1.PostAsJsonAsync($"groups/{groupId}/members", membershipRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify role was updated
        var getResponse = await client1.GetAsync($"groups/{groupId}", TestContext.Current.CancellationToken);
        var group = await getResponse.Content.ReadFromJsonAsync<GroupResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(group);
        Assert.Contains(group!.Members, m => m.UserId == userId2 && m.Role == GroupMembershipRole.Administrator);
    }

    [Fact]
    public async Task UpdateMemberRole_NonExistingGroup_ReturnsNotFound()
    {
        var (client, _, _, _) = await CreateAuthenticatedClient();
        var nonExistingId = 999999;
        var membershipRequest = new GroupMembershipRequest(1, GroupMembershipRole.Member);

        var response = await client.PostAsJsonAsync($"groups/{nonExistingId}/members", membershipRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMemberRole_OtherUsersGroup_ReturnsNotFound()
    {
        var (client1, _, _, _) = await CreateAuthenticatedClient();
        var groupId = await CreateGroup(client1, TestDataGenerator.UniqueGroupName());

        var (client2, _, _, _) = await CreateAuthenticatedClient();
        var membershipRequest = new GroupMembershipRequest(1, GroupMembershipRole.Member);

        var response = await client2.PostAsJsonAsync($"groups/{groupId}/members", membershipRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMemberRole_WithoutAuthentication_ReturnsUnauthorized()
    {
        var (authClient, _, _, _) = await CreateAuthenticatedClient();
        var groupId = await CreateGroup(authClient, TestDataGenerator.UniqueGroupName());

        using var unauthClient = _fixture.CreateClient();
        var membershipRequest = new GroupMembershipRequest(1, GroupMembershipRole.Member);

        var response = await unauthClient.PostAsJsonAsync($"groups/{groupId}/members", membershipRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RemoveMember_ExistingMember_ReturnsNoContent()
    {
        var (client1, _, _, _) = await CreateAuthenticatedClient();
        var groupId = await CreateGroup(client1, TestDataGenerator.UniqueGroupName());

        // Create second user and join via link
        var (client2, _, userId2, _) = await CreateAuthenticatedClient();
        
        // Get join link
        var linkResponse = await client1.GetAsync($"groups/{groupId}/join-link", TestContext.Current.CancellationToken);
        var joinLink = await linkResponse.Content.ReadFromJsonAsync<GroupJoinLinkResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(joinLink);
        
        // Join group using token
        await client2.PostAsync($"groups/join/{joinLink!.Token}", null, TestContext.Current.CancellationToken);

        // Remove member
        var response = await client1.DeleteAsync($"groups/{groupId}/members/{userId2}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify member was removed
        var getResponse = await client1.GetAsync($"groups/{groupId}", TestContext.Current.CancellationToken);
        var group = await getResponse.Content.ReadFromJsonAsync<GroupResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(group);
        Assert.DoesNotContain(group!.Members, m => m.UserId == userId2);
    }

    [Fact]
    public async Task RemoveMember_NonExistingGroup_ReturnsNotFound()
    {
        var (client, _, _, _) = await CreateAuthenticatedClient();
        var nonExistingId = 999999;

        var response = await client.DeleteAsync($"groups/{nonExistingId}/members/1", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RemoveMember_OtherUsersGroup_ReturnsNotFound()
    {
        var (client1, _, _, _) = await CreateAuthenticatedClient();
        var groupId = await CreateGroup(client1, TestDataGenerator.UniqueGroupName());

        var (client2, _, _, _) = await CreateAuthenticatedClient();

        var response = await client2.DeleteAsync($"groups/{groupId}/members/1", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RemoveMember_WithoutAuthentication_ReturnsUnauthorized()
    {
        var (authClient, _, _, _) = await CreateAuthenticatedClient();
        var groupId = await CreateGroup(authClient, TestDataGenerator.UniqueGroupName());

        using var unauthClient = _fixture.CreateClient();

        var response = await unauthClient.DeleteAsync($"groups/{groupId}/members/1", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Join Link Tests

    [Fact]
    public async Task GetJoinLink_ForOwnGroup_ReturnsLink()
    {
        var (client, _, _, _) = await CreateAuthenticatedClient();
        var groupId = await CreateGroup(client, TestDataGenerator.UniqueGroupName());

        var response = await client.GetAsync($"groups/{groupId}/join-link", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var joinLink = await response.Content.ReadFromJsonAsync<GroupJoinLinkResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(joinLink);
        Assert.False(string.IsNullOrWhiteSpace(joinLink!.Token));
        Assert.True(joinLink.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task GetJoinLink_NonExistingGroup_ReturnsNotFound()
    {
        var (client, _, _, _) = await CreateAuthenticatedClient();
        var nonExistingId = 999999;

        var response = await client.GetAsync($"groups/{nonExistingId}/join-link", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetJoinLink_OtherUsersGroup_ReturnsNotFound()
    {
        var (client1, _, _, _) = await CreateAuthenticatedClient();
        var groupId = await CreateGroup(client1, TestDataGenerator.UniqueGroupName());

        var (client2, _, _, _) = await CreateAuthenticatedClient();

        var response = await client2.GetAsync($"groups/{groupId}/join-link", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetJoinLink_WithoutAuthentication_ReturnsUnauthorized()
    {
        var (authClient, _, _, _) = await CreateAuthenticatedClient();
        var groupId = await CreateGroup(authClient, TestDataGenerator.UniqueGroupName());

        using var unauthClient = _fixture.CreateClient();

        var response = await unauthClient.GetAsync($"groups/{groupId}/join-link", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task JoinByToken_WithValidToken_ReturnsGroup()
    {
        var (client1, _, _, _) = await CreateAuthenticatedClient();
        var groupId = await CreateGroup(client1, TestDataGenerator.UniqueGroupName());

        // Get join link
        var linkResponse = await client1.GetAsync($"groups/{groupId}/join-link", TestContext.Current.CancellationToken);
        var joinLink = await linkResponse.Content.ReadFromJsonAsync<GroupJoinLinkResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(joinLink);

        // Create second user
        var (client2, _, _, _) = await CreateAuthenticatedClient();

        // Join group using token
        var response = await client2.PostAsync($"groups/join/{joinLink!.Token}", null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var group = await response.Content.ReadFromJsonAsync<GroupResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(group);
        Assert.Equal(groupId, group!.Id);

        // Verify user was added to group
        var getResponse = await client1.GetAsync($"groups/{groupId}", TestContext.Current.CancellationToken);
        var updatedGroup = await getResponse.Content.ReadFromJsonAsync<GroupResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(updatedGroup);
        Assert.Equal(2, updatedGroup!.Members.Count); // Creator + new member
    }

    [Fact]
    public async Task JoinByToken_WithInvalidToken_ReturnsBadRequest()
    {
        var (client, _, _, _) = await CreateAuthenticatedClient();
        var invalidToken = Guid.NewGuid().ToString("N");

        var response = await client.PostAsync($"groups/join/{invalidToken}", null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task JoinByToken_WithoutAuthentication_ReturnsUnauthorized()
    {
        var (authClient, _, _, _) = await CreateAuthenticatedClient();
        var groupId = await CreateGroup(authClient, TestDataGenerator.UniqueGroupName());

        var linkResponse = await authClient.GetAsync($"groups/{groupId}/join-link", TestContext.Current.CancellationToken);
        var joinLink = await linkResponse.Content.ReadFromJsonAsync<GroupJoinLinkResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(joinLink);

        using var unauthClient = _fixture.CreateClient();

        var response = await unauthClient.PostAsync($"groups/join/{joinLink!.Token}", null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task JoinByToken_AlreadyMember_StillReturnsOk()
    {
        var (client, _, _, _) = await CreateAuthenticatedClient();
        var groupId = await CreateGroup(client, TestDataGenerator.UniqueGroupName());

        // Get join link
        var linkResponse = await client.GetAsync($"groups/{groupId}/join-link", TestContext.Current.CancellationToken);
        var joinLink = await linkResponse.Content.ReadFromJsonAsync<GroupJoinLinkResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(joinLink);

        // Try to join own group
        var response = await client.PostAsync($"groups/join/{joinLink!.Token}", null, TestContext.Current.CancellationToken);

        // Should succeed (idempotent operation)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion
}
