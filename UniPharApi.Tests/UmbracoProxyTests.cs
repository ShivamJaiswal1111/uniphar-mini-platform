using System.Net;

namespace UniPharApi.Tests;

public class UmbracoProxyTests : IClassFixture<CustomWebApplicationFactory>
{

    // //Umbraco failures translate correctly — 500→502, 404→404, network exception→502, timeout→504 — plus a culture-fallback check
    //  //(missing translation → exactly 2 upstream calls, English retry, correct host header)
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public UmbracoProxyTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Page_WhenUmbracoReturns500_ApiReturns502()
    {
        _factory.Umbraco.Reset();
        _factory.Umbraco.Responder = _ => new HttpResponseMessage(HttpStatusCode.InternalServerError);

        var response = await _client.GetAsync("/api/uniphar-group/page/broken-page");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        Assert.Contains("unavailable", body);
    }

    [Fact]
    public async Task Page_WhenUmbracoReturns404_ApiReturns404()
    {
        _factory.Umbraco.Reset();
        _factory.Umbraco.Responder = _ => new HttpResponseMessage(HttpStatusCode.NotFound);

        var response = await _client.GetAsync("/api/uniphar-group/page/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Page_WhenUmbracoIsUnreachable_ApiReturns502()
    {
        _factory.Umbraco.Reset();
        _factory.Umbraco.Responder = _ => throw new HttpRequestException("Connection refused");

        var response = await _client.GetAsync("/api/uniphar-group/page/unreachable-page");

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }

    [Fact]
    public async Task Page_WhenUmbracoTimesOut_ApiReturns504()
    {
        _factory.Umbraco.Reset();
        _factory.Umbraco.Responder = _ => throw new TaskCanceledException("timeout", new TimeoutException());

        var response = await _client.GetAsync("/api/uniphar-group/page/slow-page");

        Assert.Equal(HttpStatusCode.GatewayTimeout, response.StatusCode);
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
            Assert.Equal("unimedtech.localhost:44335", r.Host);
        });
    }
}