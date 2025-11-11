using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LiquidCode.IntegrationTests.Infrastructure;
using LiquidCode.Api.Authentication.Requests;
using LiquidCode.Api.Authentication.Responses;

namespace LiquidCode.IntegrationTests.Tests;

[Collection(IntegrationTestCollection.Name)]
public class AuthenticationControllerTests
{
    private readonly IntegrationTestFixture _fixture;
    private static readonly JsonSerializerOptions JsonOptions = TestJson.Default;

    public AuthenticationControllerTests(IntegrationTestFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Register_WithValidRequest_ReturnsTokens()
    {
        using var client = _fixture.CreateClient();
        var username = TestDataGenerator.UniqueUsername("auth_register");
        var password = TestDataGenerator.ValidPassword();
        var request = new RegisterRequest(username, TestDataGenerator.EmailFor(username), password);

        var response = await client.PostAsJsonAsync("authentication/register", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var tokens = await response.Content.ReadFromJsonAsync<AuthTokensResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(tokens);
        Assert.False(string.IsNullOrWhiteSpace(tokens!.Jwt));
        Assert.False(string.IsNullOrWhiteSpace(tokens.RefreshToken));
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ReturnsBadRequest()
    {
        using var client = _fixture.CreateClient();
        var username = TestDataGenerator.UniqueUsername("auth_duplicate");
        var password = TestDataGenerator.ValidPassword();
        var request = new RegisterRequest(username, TestDataGenerator.EmailFor(username), password);

        var first = await client.PostAsJsonAsync("authentication/register", request, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var second = await client.PostAsJsonAsync("authentication/register", request, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokens()
    {
        using var client = _fixture.CreateClient();
        var username = TestDataGenerator.UniqueUsername("auth_login");
        var password = TestDataGenerator.ValidPassword();
        var registerRequest = new RegisterRequest(username, TestDataGenerator.EmailFor(username), password);
        var registerResponse = await client.PostAsJsonAsync("authentication/register", registerRequest, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var loginRequest = new LoginRequest(username, password);
        var loginResponse = await client.PostAsJsonAsync("authentication/login", loginRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var tokens = await loginResponse.Content.ReadFromJsonAsync<AuthTokensResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(tokens);
        Assert.False(string.IsNullOrWhiteSpace(tokens!.Jwt));
        Assert.False(string.IsNullOrWhiteSpace(tokens.RefreshToken));
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        using var client = _fixture.CreateClient();
        var username = TestDataGenerator.UniqueUsername("auth_login_invalid");
        var password = TestDataGenerator.ValidPassword();
        var registerRequest = new RegisterRequest(username, TestDataGenerator.EmailFor(username), password);
        var registerResponse = await client.PostAsJsonAsync("authentication/register", registerRequest, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var loginRequest = new LoginRequest(username, $"{password}wrong");
        var loginResponse = await client.PostAsJsonAsync("authentication/login", loginRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewTokens()
    {
        using var client = _fixture.CreateClient();
        var username = TestDataGenerator.UniqueUsername("auth_refresh");
        var password = TestDataGenerator.ValidPassword();
        var registerRequest = new RegisterRequest(username, TestDataGenerator.EmailFor(username), password);
        var registerResponse = await client.PostAsJsonAsync("authentication/register", registerRequest, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var loginRequest = new LoginRequest(username, password);
        var loginResponse = await client.PostAsJsonAsync("authentication/login", loginRequest, TestContext.Current.CancellationToken);
        var tokens = await loginResponse.Content.ReadFromJsonAsync<AuthTokensResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(tokens);

        var refreshRequest = new RefreshTokenRequest(tokens!.RefreshToken);
        var refreshResponse = await client.PostAsJsonAsync("authentication/refresh", refreshRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        var refreshed = await refreshResponse.Content.ReadFromJsonAsync<AuthTokensResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(refreshed);
        Assert.NotEqual(tokens.Jwt, refreshed!.Jwt);
        Assert.NotEqual(tokens.RefreshToken, refreshed.RefreshToken);
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_ReturnsUnauthorized()
    {
        using var client = _fixture.CreateClient();
        var invalidToken = Guid.NewGuid().ToString("N");

        var refreshRequest = new RefreshTokenRequest(invalidToken);
        var response = await client.PostAsJsonAsync("authentication/refresh", refreshRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WhoAmI_WithAuthentication_ReturnsUsername()
    {
        using var client = _fixture.CreateClient();
        var username = TestDataGenerator.UniqueUsername("auth_whoami");
        var password = TestDataGenerator.ValidPassword();
        var registerRequest = new RegisterRequest(username, TestDataGenerator.EmailFor(username), password);

        var registerResponse = await client.PostAsJsonAsync("authentication/register", registerRequest, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
        var registerTokens = await registerResponse.Content.ReadFromJsonAsync<AuthTokensResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(registerTokens);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", registerTokens!.Jwt);

        var whoAmIResponse = await client.GetAsync("authentication/whoami", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, whoAmIResponse.StatusCode);

        var body = await whoAmIResponse.Content.ReadFromJsonAsync<WhoAmIResponseDto>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(username, body!.Username);
    }

    [Fact]
    public async Task WhoAmI_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var client = _fixture.CreateClient();

        var response = await client.GetAsync("authentication/whoami", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
