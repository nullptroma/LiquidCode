using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LiquidCode.IntegrationTests.Infrastructure;
using Xunit;

[Collection(IntegrationTestCollection.Name)]
public class MissionsControllerTests
{
    private readonly IntegrationTestFixture _fixture;

    public MissionsControllerTests(IntegrationTestFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Delete_ExistingMission_ReturnsNoContent()
    {
        var (client, _, userId, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "missions_delete_existing");
        var missionId = await TestClientHelper.CreateMissionInDatabaseAsync(_fixture, userId);

        var response = await client.DeleteAsync($"missions/{missionId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await client.GetAsync($"missions/{missionId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_NonExistingMission_ReturnsNotFound()
    {
        var (client, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "missions_delete_missing");
        var nonExistingId = 999999;

        var response = await client.DeleteAsync($"missions/{nonExistingId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_OtherUsersMission_ReturnsNotFound()
    {
        var (client1, _, userId1, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "missions_delete_owner");
        var missionId = await TestClientHelper.CreateMissionInDatabaseAsync(_fixture, userId1);

        var (client2, _, _, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "missions_delete_other");
        var response = await client2.DeleteAsync($"missions/{missionId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithoutAuthentication_ReturnsUnauthorized()
    {
        var (client, _, userId, _) = await TestClientHelper.CreateAuthenticatedUserAsync(_fixture, "missions_delete_unauth");
        var missionId = await TestClientHelper.CreateMissionInDatabaseAsync(_fixture, userId);

        using var unauthClient = _fixture.CreateClient();
        var response = await unauthClient.DeleteAsync($"missions/{missionId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
