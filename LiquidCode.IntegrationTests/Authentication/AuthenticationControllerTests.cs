using System.Net;
using System.Threading.Tasks;
using LiquidCode.IntegrationTests.Infrastructure;

namespace LiquidCode.IntegrationTests.Authentication;

[Collection(IntegrationTestCollection.Name)]
public class AuthenticationControllerTests
{
    private readonly IntegrationTestFixture _fixture;

    public AuthenticationControllerTests(IntegrationTestFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task WhoAmI_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var client = _fixture.CreateClient();

    var response = await client.GetAsync("authentication/whoami", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
