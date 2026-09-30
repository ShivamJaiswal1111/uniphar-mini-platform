using System.Net;
using System.Text;

namespace UniPharApi.Tests;

public class CacheFreshnessTests
{
    private const string PageJson =
        "{\"id\":\"s1\",\"name\":\"Page\"," +
        "\"route\":{\"path\":\"/page/\",\"startItem\":{\"id\":\"g1\",\"path\":\"uniphar-group\"}}," +
        "\"properties\":{}}";

    private static HttpResponseMessage Json(string body) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

    private static async Task PublishWebhook(HttpClient client)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/webhook/content-published")
        {
            Content = new StringContent(
                "{\"eventName\":\"ContentPublished\",\"contentId\":\"1\",\"contentTypeAlias\":\"sustainabilityPage\"}",
                Encoding.UTF8, "application/json")
        };
        request.Headers.Add("X-Api-Key", CustomWebApplicationFactory.TestApiKey);
        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/uniphar-group/sustainability?culture=en-US")]
    [InlineData("/api/investors/overview?culture=en-US")]
    public async Task Page_ServedFromCache_ThenRefetchedAfterWebhook(string url)
    {
        using var factory = new CustomWebApplicationFactory();
        factory.Umbraco.Responder = _ => Json(PageJson);
        var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(url)).StatusCode);
        Assert.Single(factory.Umbraco.Requests);          // second call was a cache hit

        await PublishWebhook(client);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(url)).StatusCode);
        Assert.Equal(2, factory.Umbraco.Requests.Count);  // version bumped, so refetched
    }
}