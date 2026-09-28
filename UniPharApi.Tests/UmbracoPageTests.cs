using System.Net;
using System.Text;
using System.Text.Json;

namespace UniPharApi.Tests;

public class UmbracoPageTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public UmbracoPageTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // Builds Umbraco Delivery API JSON in the shape UmbracoMapper.MapToPage expects
    private static string PageJson(string name, string routePath, string brand, string heroHeading) =>
        JsonSerializer.Serialize(new
        {
            id = "11111111-1111-1111-1111-111111111111",
            name,
            contentType = "standardPage",
            route = new
            {
                path = routePath,
                startItem = new { path = brand }
            },
            properties = new
            {
                heroHeading,
                heroImage = new[] { new { url = "/media/abc123/hero.jpg" } }
            }
        });

    private void UmbracoReturns(string json)
    {
        _factory.Umbraco.Reset();
        _factory.Umbraco.Responder = _ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    [Fact]
    public async Task Page_WhenUmbracoReturnsJson_ReturnsMappedPage()
    {
        UmbracoReturns(PageJson("About Us", "/about-us/", "uniphar-group", "Who we are"));

        var response = await _client.GetAsync("/api/uniphar-group/page/about-us");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        Assert.Equal("About Us", root.GetProperty("title").GetString());
        Assert.Equal("about-us", root.GetProperty("slug").GetString());
        Assert.Equal("uniphar-group", root.GetProperty("brandSlug").GetString());
        Assert.Equal("Who we are", root.GetProperty("heroHeading").GetString());
        Assert.Equal("en-US", root.GetProperty("culture").GetString());
        Assert.Contains("/api/media/abc123/hero.jpg", root.GetProperty("heroImageUrl").GetString());
    }

    [Fact]
    public async Task Page_SecondIdenticalRequest_IsServedFromCache()
    {
        UmbracoReturns(PageJson("Cached Page", "/cached-page/", "uniphar-group", "Hello"));

        var first = await _client.GetAsync("/api/uniphar-group/page/cached-page");
        var second = await _client.GetAsync("/api/uniphar-group/page/cached-page");

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Single(_factory.Umbraco.Requests);
    }

    [Fact]
    public async Task Home_WithFrenchCulture_PassesCultureThroughAndReturnsRootSlug()
    {
        UmbracoReturns(PageJson("Uniphar Medtech", "/", "uniphar-medtech", "Solutions de Technologie Médicale"));

        var response = await _client.GetAsync("/api/uniphar-medtech/home?culture=fr");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var request = Assert.Single(_factory.Umbraco.Requests);
        Assert.Equal("fr", request.AcceptLanguage);
        Assert.Equal("/umbraco/delivery/api/v2/content/item/", request.PathAndQuery);

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        Assert.Equal("uniphar-medtech", root.GetProperty("slug").GetString());
        Assert.Equal("fr", root.GetProperty("culture").GetString());
        Assert.Equal("Solutions de Technologie Médicale", root.GetProperty("heroHeading").GetString());
    }
}