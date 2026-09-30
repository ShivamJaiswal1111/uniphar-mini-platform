using System.Net;
using System.Text;

namespace UniPharApi.Tests;

public class BlogEmptyCacheTests
{
    private static HttpResponseMessage Json(string body) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };


    private const string OnePostJson =
    "{\"total\":1,\"items\":[{\"id\":\"p1\",\"contentType\":\"blogPost\",\"name\":\"Test Post\"," +
    "\"createDate\":\"2026-01-01T00:00:00Z\",\"updateDate\":\"2026-01-01T00:00:00Z\"," +
    "\"route\":{\"path\":\"/blog-posts/test-post/\",\"startItem\":{\"id\":\"g1\",\"path\":\"uniphar-group\"}}," +
    "\"properties\":{}}]}";

    [Fact]
    public async Task NonEmptyBlogList_IsCached()
    {
        using var factory = new CustomWebApplicationFactory();
        factory.Umbraco.Responder = _ => Json(OnePostJson);
        var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/blog")).StatusCode);
        var afterFirst = factory.Umbraco.Requests.Count;
        Assert.True(afterFirst >= 1);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/blog")).StatusCode);

        // Control: a non-empty list is served from cache, so no new Umbraco request.
        Assert.Equal(afterFirst, factory.Umbraco.Requests.Count);
    }
    [Fact]
    public async Task EmptyBlogList_IsReturned_ButNotCached()
    {
        using var factory = new CustomWebApplicationFactory();
        factory.Umbraco.Responder = _ => Json("{\"total\":0,\"items\":[]}");
        var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/blog")).StatusCode);
        var afterFirst = factory.Umbraco.Requests.Count;
        Assert.True(afterFirst >= 1);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/blog")).StatusCode);

        // The second call must reach Umbraco again, so the empty list was not cached.
        Assert.True(factory.Umbraco.Requests.Count > afterFirst);
    }

    
}