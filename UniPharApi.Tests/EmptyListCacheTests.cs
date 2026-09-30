using System.Net;
using System.Text;

namespace UniPharApi.Tests;

public class EmptyListCacheTests
{
    private const string EmptyListJson = "{\"total\":0,\"items\":[]}";

    private const string OneItemJson =
        "{\"total\":1,\"items\":[{\"id\":\"b1\",\"name\":\"Uniphar Group\"," +
        "\"route\":{\"path\":\"/\",\"startItem\":{\"id\":\"b1\",\"path\":\"uniphar-group\"}}," +
        "\"properties\":{}}]}";

    private static HttpResponseMessage Json(string body) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

    private const string Url = "/api/brands?culture=en-US";

    [Fact]
    public async Task EmptyList_IsReturned_ButNotCached()
    {
        using var factory = new CustomWebApplicationFactory();
        factory.Umbraco.Responder = _ => Json(EmptyListJson);
        var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(Url)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(Url)).StatusCode);

        // Both calls reached Umbraco, so the empty result was not cached.
        Assert.Equal(2, factory.Umbraco.Requests.Count);
    }

    [Fact]
    public async Task NonEmptyList_IsCached()
    {
        using var factory = new CustomWebApplicationFactory();
        factory.Umbraco.Responder = _ => Json(OneItemJson);
        var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(Url)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(Url)).StatusCode);

        // Control: a non-empty list is still served from cache on the second call.
        Assert.Single(factory.Umbraco.Requests);
    }
}