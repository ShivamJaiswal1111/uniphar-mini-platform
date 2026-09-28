using System.Net;

namespace UniPharApi.Tests;

public class UmbracoProxyTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public UmbracoProxyTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Page_WhenUmbracoReturns500_ApiReturns500()
    {
        _factory.Umbraco.Reset();
        _factory.Umbraco.Responder = _ => new HttpResponseMessage(HttpStatusCode.InternalServerError);

        var response = await _client.GetAsync("/api/uniphar-group/page/broken-page");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task Page_WhenCultureMissing_RetriesInEnUsWithBrandHost()
    {
        _factory.Umbraco.Reset();
        _factory.Umbraco.Responder = _ => new HttpResponseMessage(HttpStatusCode.NotFound);

        await _client.GetAsync("/api/uniphar-medtech/page/services/cardiac?culture=fr");

        var requests = _factory.Umbraco.Requests;

        Assert.Equal(2, requests.Count);
        Assert.Equal("fr", requests[0].AcceptLanguage);
        Assert.Equal("en-US", requests[1].AcceptLanguage);
        Assert.All(requests, r =>
        {
            Assert.Equal("/umbraco/delivery/api/v2/content/item/services/cardiac", r.PathAndQuery);
            Assert.Equal("unimedtech.localhost:1", r.Host);
        });
    }
}