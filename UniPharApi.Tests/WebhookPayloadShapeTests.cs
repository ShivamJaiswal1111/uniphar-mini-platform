using System.Net;
using System.Text;

namespace UniPharApi.Tests;

public class WebhookPayloadShapeTests
{
    private static HttpRequestMessage Post(string body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/webhook/content-published")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("X-Api-Key", CustomWebApplicationFactory.TestApiKey);
        request.Headers.Add("Umb-Webhook-Event", "Umbraco.ContentPublish");
        return request;
    }

    [Fact]
    public async Task RealUmbracoShape_Returns200()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.SendAsync(Post(
            "{\"id\":\"abc\",\"contentType\":\"homePage\",\"name\":\"Home\",\"properties\":{}}"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UnreadableBody_StillReturns200()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.SendAsync(Post("not json"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}