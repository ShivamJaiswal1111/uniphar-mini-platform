using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace UniPharApi.Tests;

public class AuthTests : IClassFixture<CustomWebApplicationFactory>
{

    // //Login rejects wrong password, issues a real JWT on success, /me rejects no token and accepts a valid one
    private readonly HttpClient _client;


    public AuthTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static StringContent LoginBody(string username, string password) => new(
        JsonSerializer.Serialize(new { username, password }),
        Encoding.UTF8,
        "application/json");

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var response = await _client.PostAsync("/api/auth/login",
            LoginBody(CustomWebApplicationFactory.TestUsername, "wrong-password"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var response = await _client.PostAsync("/api/auth/login",
            LoginBody(CustomWebApplicationFactory.TestUsername, CustomWebApplicationFactory.TestPassword));
        var json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var doc = JsonDocument.Parse(json);
        var token = doc.RootElement.GetProperty("token").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithValidToken_Returns200()
    {
        var loginResponse = await _client.PostAsync("/api/auth/login",
            LoginBody(CustomWebApplicationFactory.TestUsername, CustomWebApplicationFactory.TestPassword));
        using var doc = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        var token = doc.RootElement.GetProperty("token").GetString();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"isAuthenticated\":true", body);
    }
}