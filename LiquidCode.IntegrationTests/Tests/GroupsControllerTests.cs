using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
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

    #region Create Tests

    [Fact]
    public async Task Create_WithValidRequest_ReturnsGroup()
    {
        var (client, username, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_create_valid");
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
        var (client, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_create_no_description");
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
        var (client, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_create_invalid_name");
        var request = new CreateGroupRequest("AB", "Description"); // Too short (min 3)

        var response = await client.PostAsJsonAsync("groups", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithTooLongName_ReturnsBadRequest()
    {
        var (client, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_create_too_long");
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
        var (client, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_get_existing");
        var groupName = TestDataGenerator.UniqueGroupName();
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(client, groupName);

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
        var (authClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_get_public");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(authClient, TestDataGenerator.UniqueGroupName());

        using var unauthClient = _fixture.CreateClient();
        var response = await unauthClient.GetAsync($"groups/{groupId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_NonExistingGroup_ReturnsNotFound()
    {
        var (client, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_get_missing");
        var nonExistingId = 999999;

        var response = await client.GetAsync($"groups/{nonExistingId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidRequest_ReturnsUpdatedGroup()
    {
        var (client, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_update_valid");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(client, $"Old Name {Guid.NewGuid():N}", "Old Description");

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
        var (client, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_update_name_only");
        var oldDescription = "Old Description";
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(client, $"Old Name {Guid.NewGuid():N}", oldDescription);

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
        var (client, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_update_description_only");
        var oldName = $"Old Name {Guid.NewGuid():N}";
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(client, oldName, "Old Description");

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
        var (client, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_update_missing");
        var nonExistingId = 999999;
        var updateRequest = new UpdateGroupRequest("New Name", "New Description");

        var response = await client.PutAsJsonAsync($"groups/{nonExistingId}", updateRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_OtherUsersGroup_ReturnsNotFound()
    {
        var (client1, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_update_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(client1, TestDataGenerator.UniqueGroupName());

        var (client2, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_update_other");
        var updateRequest = new UpdateGroupRequest("Hacked Name", null);

        var response = await client2.PutAsJsonAsync($"groups/{groupId}", updateRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithoutAuthentication_ReturnsUnauthorized()
    {
        var (authClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_update_unauth");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(authClient, TestDataGenerator.UniqueGroupName());

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
        var (client, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_delete_existing");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(client, TestDataGenerator.UniqueGroupName());

        var response = await client.DeleteAsync($"groups/{groupId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify group is deleted
        var getResponse = await client.GetAsync($"groups/{groupId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_NonExistingGroup_ReturnsNotFound()
    {
        var (client, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_delete_missing");
        var nonExistingId = 999999;

        var response = await client.DeleteAsync($"groups/{nonExistingId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_OtherUsersGroup_ReturnsNotFound()
    {
        var (client1, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_delete_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(client1, TestDataGenerator.UniqueGroupName());

        var (client2, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_delete_other");

        var response = await client2.DeleteAsync($"groups/{groupId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithoutAuthentication_ReturnsUnauthorized()
    {
        var (authClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_delete_unauth");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(authClient, TestDataGenerator.UniqueGroupName());

        using var unauthClient = _fixture.CreateClient();

        var response = await unauthClient.DeleteAsync($"groups/{groupId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region GetMyGroups Tests

    [Fact]
    public async Task GetMyGroups_WithMultipleGroups_ReturnsAllGroups()
    {
        var (client, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_my_multiple");

        var (groupId1, _) = await TestClientHelper.CreateGroupAsync(client, $"Group 1 {Guid.NewGuid():N}");
        var (groupId2, _) = await TestClientHelper.CreateGroupAsync(client, $"Group 2 {Guid.NewGuid():N}");
        var (groupId3, _) = await TestClientHelper.CreateGroupAsync(client, $"Group 3 {Guid.NewGuid():N}");

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
        var (client, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_my_empty");

        var response = await client.GetAsync("groups/my", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var page = await response.Content.ReadFromJsonAsync<GroupsPageResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(page);
        Assert.Empty(page!.Groups);
    }

    [Fact]
    public async Task GetMyGroups_WithPagination_ReturnsCorrectPage()
    {
        var (client, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_my_pagination");

        // Create 15 groups
        for (int i = 0; i < 15; i++)
        {
            await TestClientHelper.CreateGroupAsync(client, $"Group {i} {Guid.NewGuid():N}");
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
        var (client1, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_member_nonexistent");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(client1, TestDataGenerator.UniqueGroupName());

        // Create second user
        var (client2, _, userId2, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_member_nonexistent_user");

        // Try to update non-existent member
        var membershipRequest = new GroupMembershipRequest(userId2, GroupMembershipRole.Member);
        var response = await client1.PostAsJsonAsync($"groups/{groupId}/members", membershipRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMemberRole_UpdateExistingMember_ReturnsNoContent()
    {
        var (client1, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_member_promote_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(client1, TestDataGenerator.UniqueGroupName());

        var (client2, _, userId2, _) = await TestClientHelper.CreateGroupMemberAsync(_fixture, client1, groupId, "groups_member_promote_member");

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
    public async Task UpdateMemberRole_AdminCannotMakeSelfCreator_ReturnsNotFound()
    {
        var (ownerClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_admin_promote_self_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(ownerClient, TestDataGenerator.UniqueGroupName());

        // Create and join a member
        var (adminClient, _, adminId, _) = await TestClientHelper.CreateGroupMemberAsync(_fixture, ownerClient, groupId, "groups_admin_promote_self_admin");

        // Owner promotes user to Administrator
        var promoteToAdminRequest = new GroupMembershipRequest(adminId, GroupMembershipRole.Administrator);
        var promoteAdminResponse = await ownerClient.PostAsJsonAsync($"groups/{groupId}/members", promoteToAdminRequest, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, promoteAdminResponse.StatusCode);

        // Admin tries to make themselves Creator -> not allowed
        var makeCreatorRequest = new GroupMembershipRequest(adminId, GroupMembershipRole.Creator | GroupMembershipRole.Administrator);
        var response = await adminClient.PostAsJsonAsync($"groups/{groupId}/members", makeCreatorRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMemberRole_AdminCannotMakeOtherCreator_ReturnsNotFound()
    {
        var (ownerClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_admin_promote_other_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(ownerClient, TestDataGenerator.UniqueGroupName());

        // Create and join two members
        var (adminClient, _, adminId, _) = await TestClientHelper.CreateGroupMemberAsync(_fixture, ownerClient, groupId, "groups_admin_promote_other_admin");
        var (memberClient2, _, memberId2, _) = await TestClientHelper.CreateGroupMemberAsync(_fixture, ownerClient, groupId, "groups_admin_promote_other_member");

        // Owner promotes adminClient to Administrator
        var promoteToAdminRequest = new GroupMembershipRequest(adminId, GroupMembershipRole.Administrator);
        var promoteAdminResponse = await ownerClient.PostAsJsonAsync($"groups/{groupId}/members", promoteToAdminRequest, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, promoteAdminResponse.StatusCode);

        // Admin tries to make another member Creator -> not allowed
        var makeCreatorRequest = new GroupMembershipRequest(memberId2, GroupMembershipRole.Creator | GroupMembershipRole.Administrator);
        var response = await adminClient.PostAsJsonAsync($"groups/{groupId}/members", makeCreatorRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMemberRole_MakeMemberCreator_OriginalCreatorLosesCreator()
    {
        var (ownerClient, _, ownerId, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_member_promote_to_creator_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(ownerClient, TestDataGenerator.UniqueGroupName());

        // Create and join a regular member
        var (memberClient, _, memberId, _) = await TestClientHelper.CreateGroupMemberAsync(_fixture, ownerClient, groupId, "groups_member_promote_to_creator_member");

        // Owner promotes member to Creator and Administrator
        var makeCreatorRequest = new GroupMembershipRequest(memberId, GroupMembershipRole.Creator);
        var promoteResponse = await ownerClient.PostAsJsonAsync($"groups/{groupId}/members", makeCreatorRequest, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, promoteResponse.StatusCode);

        // Verify new member is Creator+Administrator and owner has lost Creator flag
        var getResponse = await ownerClient.GetAsync($"groups/{groupId}", TestContext.Current.CancellationToken);
        var group = await getResponse.Content.ReadFromJsonAsync<GroupResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(group);

        // Member should be Administrator + Creator
        Assert.Contains(group!.Members, m => m.UserId == memberId && m.Role.HasFlag(GroupMembershipRole.Creator) && m.Role.HasFlag(GroupMembershipRole.Administrator));

        // Owner should no longer have Creator flag (but keep Administrator)
        Assert.Contains(group.Members, m => m.UserId == ownerId && m.Role.HasFlag(GroupMembershipRole.Administrator) && !m.Role.HasFlag(GroupMembershipRole.Creator));
    }

    [Fact]
    public async Task UpdateMemberRole_MakeAdminCreator_OriginalCreatorLosesCreator()
    {
        var (ownerClient, _, ownerId, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_member_promote_admin_to_creator_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(ownerClient, TestDataGenerator.UniqueGroupName());

        // Create and join a member
        var (adminClient, _, adminId, _) = await TestClientHelper.CreateGroupMemberAsync(_fixture, ownerClient, groupId, "groups_member_promote_admin_to_creator_admin");

        // Owner promotes member to Administrator first
        var promoteToAdminRequest = new GroupMembershipRequest(adminId, GroupMembershipRole.Administrator);
        var promoteAdminResponse = await ownerClient.PostAsJsonAsync($"groups/{groupId}/members", promoteToAdminRequest, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, promoteAdminResponse.StatusCode);

        // Now owner promotes admin to Creator (should also give Administrator flag)
        var makeCreatorRequest = new GroupMembershipRequest(adminId, GroupMembershipRole.Creator);
        var promoteResponse = await ownerClient.PostAsJsonAsync($"groups/{groupId}/members", makeCreatorRequest, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, promoteResponse.StatusCode);

        // Verify admin is Creator+Administrator and owner has lost Creator flag
        var getResponse = await ownerClient.GetAsync($"groups/{groupId}", TestContext.Current.CancellationToken);
        var group = await getResponse.Content.ReadFromJsonAsync<GroupResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(group);

        Assert.Contains(group!.Members, m => m.UserId == adminId && m.Role.HasFlag(GroupMembershipRole.Creator) && m.Role.HasFlag(GroupMembershipRole.Administrator));
        Assert.Contains(group.Members, m => m.UserId == ownerId && m.Role.HasFlag(GroupMembershipRole.Administrator) && !m.Role.HasFlag(GroupMembershipRole.Creator));
    }

    [Fact]
    public async Task UpdateMemberRole_NonExistingGroup_ReturnsNotFound()
    {
        var (client, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_member_missing_group");
        var nonExistingId = 999999;
        var membershipRequest = new GroupMembershipRequest(1, GroupMembershipRole.Member);

        var response = await client.PostAsJsonAsync($"groups/{nonExistingId}/members", membershipRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }


    [Fact]
    public async Task UpdateMemberRole_OtherUsersGroup_ReturnsNotFound()
    {
        var (client1, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_member_foreign_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(client1, TestDataGenerator.UniqueGroupName());

        var (client2, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_member_foreign_user");
        var membershipRequest = new GroupMembershipRequest(1, GroupMembershipRole.Member);

        var response = await client2.PostAsJsonAsync($"groups/{groupId}/members", membershipRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMemberRole_WithoutAuthentication_ReturnsUnauthorized()
    {
        var (authClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_member_unauth");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(authClient, TestDataGenerator.UniqueGroupName());

        using var unauthClient = _fixture.CreateClient();
        var membershipRequest = new GroupMembershipRequest(1, GroupMembershipRole.Member);

        var response = await unauthClient.PostAsJsonAsync($"groups/{groupId}/members", membershipRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMemberRole_CannotDemoteCreator_ByCreator_ReturnsNotFound()
    {
        var (client, _, creatorId, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_member_creator_self_demote");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(client, TestDataGenerator.UniqueGroupName());

        // Creator tries to remove Creator flag from themselves -> not allowed
        var membershipRequest = new GroupMembershipRequest(creatorId, GroupMembershipRole.Administrator);

        var response = await client.PostAsJsonAsync($"groups/{groupId}/members", membershipRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMemberRole_CannotDemoteCreator_ByOtherAdmin_ReturnsNotFound()
    {
        // Owner creates group
        var (ownerClient, _, ownerId, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_member_creator_protected_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(ownerClient, TestDataGenerator.UniqueGroupName());

        // Create and join second user
        var (adminClient, _, userId2, _) = await TestClientHelper.CreateGroupMemberAsync(_fixture, ownerClient, groupId, "groups_member_creator_protected_admin");

        // Owner promotes second user to Administrator
        var promoteRequest = new GroupMembershipRequest(userId2, GroupMembershipRole.Administrator);
        var promoteResponse = await ownerClient.PostAsJsonAsync($"groups/{groupId}/members", promoteRequest, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, promoteResponse.StatusCode);

        // Second user (now admin) tries to demote the owner (remove Creator flag)
        var demoteRequest = new GroupMembershipRequest(ownerId, GroupMembershipRole.Administrator);
        var response = await adminClient.PostAsJsonAsync($"groups/{groupId}/members", demoteRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RemoveMember_ExistingMember_ReturnsNoContent()
    {
        var (client1, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_remove_member_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(client1, TestDataGenerator.UniqueGroupName());

        var (client2, _, userId2, _) = await TestClientHelper.CreateGroupMemberAsync(_fixture, client1, groupId, "groups_remove_member_member");

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
        var (client, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_remove_missing_group");
        var nonExistingId = 999999;

        var response = await client.DeleteAsync($"groups/{nonExistingId}/members/1", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RemoveMember_OtherUsersGroup_ReturnsNotFound()
    {
        var (client1, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_remove_foreign_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(client1, TestDataGenerator.UniqueGroupName());

        var (client2, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_remove_foreign_user");

        var response = await client2.DeleteAsync($"groups/{groupId}/members/1", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RemoveMember_WithoutAuthentication_ReturnsUnauthorized()
    {
        var (authClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_remove_unauth");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(authClient, TestDataGenerator.UniqueGroupName());

        using var unauthClient = _fixture.CreateClient();

        var response = await unauthClient.DeleteAsync($"groups/{groupId}/members/1", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RemoveMember_CannotRemoveCreator_ByCreator_ReturnsNotFound()
    {
        var (client, _, creatorId, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_remove_creator_self");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(client, TestDataGenerator.UniqueGroupName());

        // Creator tries to remove themselves
        var response = await client.DeleteAsync($"groups/{groupId}/members/{creatorId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RemoveMember_CannotRemoveCreator_ByOtherAdmin_ReturnsNotFound()
    {
        var (ownerClient, _, ownerId, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_remove_creator_protection_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(ownerClient, TestDataGenerator.UniqueGroupName());

        var (adminClient, _, userId2, _) = await TestClientHelper.CreateGroupMemberAsync(_fixture, ownerClient, groupId, "groups_remove_creator_protection_admin");

        // Owner promotes second user to Administrator
        var promoteRequest = new GroupMembershipRequest(userId2, GroupMembershipRole.Administrator);
        var promoteResponse = await ownerClient.PostAsJsonAsync($"groups/{groupId}/members", promoteRequest, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, promoteResponse.StatusCode);

        // Admin tries to remove creator
        var response = await adminClient.DeleteAsync($"groups/{groupId}/members/{ownerId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RemoveMember_Self_AsMember_ReturnsNoContent()
    {
        var (ownerClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_remove_self_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(ownerClient, TestDataGenerator.UniqueGroupName());

        // Create and join a member
        var (memberClient, _, memberId, _) = await TestClientHelper.CreateGroupMemberAsync(_fixture, ownerClient, groupId, "groups_remove_self_member");

        // Member removes themselves
        var response = await memberClient.DeleteAsync($"groups/{groupId}/members/{memberId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify member was removed
        var getResponse = await ownerClient.GetAsync($"groups/{groupId}", TestContext.Current.CancellationToken);
        var group = await getResponse.Content.ReadFromJsonAsync<GroupResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(group);
        Assert.DoesNotContain(group!.Members, m => m.UserId == memberId);
    }

    [Fact]
    public async Task RemoveMember_Self_AsAdmin_ReturnsNoContent()
    {
        var (ownerClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_remove_self_owner2");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(ownerClient, TestDataGenerator.UniqueGroupName());

        // Create and join a member
        var (adminClient, _, adminId, _) = await TestClientHelper.CreateGroupMemberAsync(_fixture, ownerClient, groupId, "groups_remove_self_admin");

        // Owner promotes them to Administrator
        var promoteRequest = new GroupMembershipRequest(adminId, GroupMembershipRole.Administrator);
        var promoteResponse = await ownerClient.PostAsJsonAsync($"groups/{groupId}/members", promoteRequest, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, promoteResponse.StatusCode);

        // Admin removes themselves
        var response = await adminClient.DeleteAsync($"groups/{groupId}/members/{adminId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify admin was removed
        var getResponse = await ownerClient.GetAsync($"groups/{groupId}", TestContext.Current.CancellationToken);
        var group = await getResponse.Content.ReadFromJsonAsync<GroupResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(group);
        Assert.DoesNotContain(group!.Members, m => m.UserId == adminId);
    }

    [Fact]
    public async Task RemoveMember_AdminRemovesMember_ReturnsNoContent()
    {
        var (ownerClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_admin_remove_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(ownerClient, TestDataGenerator.UniqueGroupName());

        // Create a regular member
        var (memberClient, _, memberId, _) = await TestClientHelper.CreateGroupMemberAsync(_fixture, ownerClient, groupId, "groups_admin_remove_member");

        // Create and promote another user to Administrator
        var (adminClient, _, adminId, _) = await TestClientHelper.CreateGroupMemberAsync(_fixture, ownerClient, groupId, "groups_admin_remove_admin");
        var promoteRequest = new GroupMembershipRequest(adminId, GroupMembershipRole.Administrator);
        var promoteResponse = await ownerClient.PostAsJsonAsync($"groups/{groupId}/members", promoteRequest, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, promoteResponse.StatusCode);

        // Admin removes the regular member
        var response = await adminClient.DeleteAsync($"groups/{groupId}/members/{memberId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify member was removed
        var getResponse = await ownerClient.GetAsync($"groups/{groupId}", TestContext.Current.CancellationToken);
        var group = await getResponse.Content.ReadFromJsonAsync<GroupResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(group);
        Assert.DoesNotContain(group!.Members, m => m.UserId == memberId);
    }

    #endregion

    #region Join Link Tests

    [Fact]
    public async Task GetJoinLink_ForOwnGroup_ReturnsLink()
    {
        var (client, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_joinlink_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(client, TestDataGenerator.UniqueGroupName());

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
        var (client, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_joinlink_missing");
        var nonExistingId = 999999;

        var response = await client.GetAsync($"groups/{nonExistingId}/join-link", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetJoinLink_OtherUsersGroup_ReturnsNotFound()
    {
        var (client1, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_joinlink_foreign_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(client1, TestDataGenerator.UniqueGroupName());

        var (client2, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_joinlink_foreign_user");

        var response = await client2.GetAsync($"groups/{groupId}/join-link", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetJoinLink_WithoutAuthentication_ReturnsUnauthorized()
    {
        var (authClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_joinlink_unauth");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(authClient, TestDataGenerator.UniqueGroupName());

        using var unauthClient = _fixture.CreateClient();

        var response = await unauthClient.GetAsync($"groups/{groupId}/join-link", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task JoinByToken_WithValidToken_ReturnsGroup()
    {
        var (client1, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_join_valid_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(client1, TestDataGenerator.UniqueGroupName());

        // Get join link
        var linkResponse = await client1.GetAsync($"groups/{groupId}/join-link", TestContext.Current.CancellationToken);
        var joinLink = await linkResponse.Content.ReadFromJsonAsync<GroupJoinLinkResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(joinLink);

        // Create second user
        var (client2, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_join_valid_member");

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
        var (client, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_join_invalid_token");
        var invalidToken = Guid.NewGuid().ToString("N");

        var response = await client.PostAsync($"groups/join/{invalidToken}", null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task JoinByToken_WithoutAuthentication_ReturnsUnauthorized()
    {
        var (authClient, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_join_unauth_owner");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(authClient, TestDataGenerator.UniqueGroupName());

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
        var (client, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "groups_join_self");
        var (groupId, _) = await TestClientHelper.CreateGroupAsync(client, TestDataGenerator.UniqueGroupName());

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
