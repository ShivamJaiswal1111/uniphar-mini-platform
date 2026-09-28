using System.Net;
using System.Text;

namespace UniPharApi.Tests;

public class WebhookTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public WebhookTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static StringContent Payload() => new(
        "{\"eventName\":\"ContentPublished\",\"contentId\":\"1\",\"contentTypeAlias\":\"homePage\"}",
        Encoding.UTF8,
        "application/json");

    [Fact]
    public async Task ContentPublished_WithoutApiKey_Returns401()
    {
        var response = await _client.PostAsync("/api/webhook/content-published", Payload());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ContentPublished_WithValidApiKey_Returns200AndBumpsVersion()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/webhook/content-published")
        {
            Content = Payload()
        };
        request.Headers.Add("X-Api-Key", CustomWebApplicationFactory.TestApiKey);

        var response = await _client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Cache invalidated", body);
    }
}