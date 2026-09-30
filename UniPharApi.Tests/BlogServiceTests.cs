using System.Net;
using System.Text;

namespace UniPharApi.Tests;

public class BlogServiceTests
{
    [Fact]
    public async Task List_WhenUmbracoFails500_Returns502()
    {
        using var factory = new CustomWebApplicationFactory();
        factory.Umbraco.Responder = _ => new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/blog");

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }

    [Fact]
    public async Task Detail_WhenUmbracoFails500_Returns502_NotNotFound()
    {
        using var factory = new CustomWebApplicationFactory();
        factory.Umbraco.Responder = _ => new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/blog/some-post");

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }

    [Fact]
    public async Task Detail_WhenPostMissing_Returns404()
    {
        using var factory = new CustomWebApplicationFactory();
        factory.Umbraco.Responder = _ => new HttpResponseMessage(HttpStatusCode.NotFound);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/blog/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task List_SendsGroupHostWithPort()
    {
        using var factory = new CustomWebApplicationFactory();
        factory.Umbraco.Responder = _ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"total\":0,\"items\":[]}", Encoding.UTF8, "application/json")
        };
        var client = factory.CreateClient();

        await client.GetAsync("/api/blog");

        Assert.Equal("uniphargroup.localhost:44335", factory.Umbraco.Requests[0].Host);
    }
}