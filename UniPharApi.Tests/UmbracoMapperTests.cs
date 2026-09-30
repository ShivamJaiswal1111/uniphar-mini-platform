using UniPharApi.Models;
using Xunit;

namespace UniPharApi.Tests;

public class UmbracoMapperTests
{
    // Builds a minimal Umbraco page JSON. $$""" lets {{x}} insert a value
    // while single { } stay literal, which JSON needs.

    // //Data mapping logic (Umbraco JSON → clean models) is correct in isolation
    private static string PageJson(string routePath, string propertiesJson = "{}") => $$"""
    {
      "contentType": "standardPage",
      "name": "Test Page",
      "id": "11111111-1111-1111-1111-111111111111",
      "route": {
        "path": "{{routePath}}",
        "startItem": { "id": "22222222-2222-2222-2222-222222222222", "path": "uniphar-group" }
      },
      "properties": {{propertiesJson}}
    }
    """;

    // ---- ResolveMediaUrls ----

    [Fact]
    public void ResolveMediaUrls_RewritesRelativeMediaSrc_ToApiProxyUrl()
    {
        var html = "<img src=\"/media/abc123/file.jpg\">";

        var result = UmbracoMapper.ResolveMediaUrls(html);

        Assert.Equal("<img src=\"http://localhost:5220/api/media/abc123/file.jpg\">", result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ResolveMediaUrls_ReturnsInputUnchanged_WhenNullOrEmpty(string? input)
    {
        var result = UmbracoMapper.ResolveMediaUrls(input!);

        Assert.Equal(input, result);
    }

    [Fact]
    public void ResolveMediaUrls_LeavesExternalImages_Untouched()
    {
        var html = "<img src=\"https://example.com/a.jpg\">";

        var result = UmbracoMapper.ResolveMediaUrls(html);

        Assert.Equal(html, result);
    }

    // ---- Slug (private ExtractSlug, tested through MapToPage) ----

    [Fact]
    public void MapToPage_RootNode_UsesBrandSlugAsSlug()
    {
        var page = UmbracoMapper.MapToPage(PageJson("/"), "en-US");

        Assert.Equal("uniphar-group", page.Slug);
    }

    [Fact]
    public void MapToPage_ChildPage_UsesLastPathSegmentAsSlug()
    {
        var page = UmbracoMapper.MapToPage(PageJson("/investors/results-centre/"), "en-US");

        Assert.Equal("results-centre", page.Slug);
    }

    // ---- Links (private ExtractLinkUrl, tested through CtaButtonLink) ----

    [Fact]
    public void MapToPage_InternalLink_ReadsNestedRoutePath()
    {
        var props = """
        { "ctaButtonLink": [ { "url": null, "route": { "path": "/about-us/" } } ] }
        """;

        var page = UmbracoMapper.MapToPage(PageJson("/", props), "en-US");

        Assert.Equal("/about-us/", page.CtaButtonLink);
    }

    [Fact]
    public void MapToPage_ExternalLink_ReadsUrlDirectly()
    {
        var props = """
        { "ctaButtonLink": [ { "url": "https://example.com/x", "route": null } ] }
        """;

        var page = UmbracoMapper.MapToPage(PageJson("/", props), "en-US");

        Assert.Equal("https://example.com/x", page.CtaButtonLink);
    }

    // ---- Missing optional fields ----

    [Fact]
    public void MapToPage_MissingOptionalProperties_GivesNullsAndEmptyList()
    {
        var page = UmbracoMapper.MapToPage(PageJson("/"), "en-US");

        Assert.Null(page.HeroHeading);
        Assert.Null(page.HeroImageUrl);
        Assert.Empty(page.FeaturedSections);
    }

     [Fact]
    public void MapKeyStats_MapsValueAndLabel_ForEachBlockItem()
    {
        var json = """
        { "keyStats": { "items": [
          { "content": { "properties": { "statValue": "130.9m", "statLabel": "EBITDA", "statIcon": [] } } },
          { "content": { "properties": { "statValue": "4.2bn",  "statLabel": "Revenue", "statIcon": [] } } }
        ] } }
        """;
        using var doc = System.Text.Json.JsonDocument.Parse(json);

        var stats = UmbracoMapper.MapKeyStats(doc.RootElement, "keyStats");

        Assert.Equal(2, stats.Count);
        Assert.Equal("130.9m", stats[0].Value);
        Assert.Equal("EBITDA", stats[0].Label);
        Assert.Equal("Revenue", stats[1].Label);
    }

    [Fact]
    public void MapKeyStats_ReturnsEmptyList_WhenPropertyMissing()
    {
        using var doc = System.Text.Json.JsonDocument.Parse("{}");

        var stats = UmbracoMapper.MapKeyStats(doc.RootElement, "keyStats");

        Assert.Empty(stats);
    }

    [Fact]
    public void MapKeyStats_EmptyIconArray_GivesNullIconUrl()
    {
        var json = """
        { "keyStats": { "items": [
          { "content": { "properties": { "statValue": "1", "statLabel": "A", "statIcon": [] } } }
        ] } }
        """;
        using var doc = System.Text.Json.JsonDocument.Parse(json);

        var stats = UmbracoMapper.MapKeyStats(doc.RootElement, "keyStats");

        Assert.Null(stats[0].IconUrl);
    }

    [Fact]
    public void MapKeyStats_WithIcon_BuildsApiProxyUrl()
    {
        var json = """
        { "keyStats": { "items": [
          { "content": { "properties": { "statValue": "1", "statLabel": "A",
              "statIcon": [ { "url": "/media/abc/icon.png" } ] } } }
        ] } }
        """;
        using var doc = System.Text.Json.JsonDocument.Parse(json);

        var stats = UmbracoMapper.MapKeyStats(doc.RootElement, "keyStats");

        Assert.Equal("http://localhost:5220/api/media/abc/icon.png", stats[0].IconUrl);
    }

    // ---- MapToInvestorOverview ----

    [Fact]
    public void MapToInvestorOverview_MapsYearAndKeyStats()
    {
        var json = """
        {
          "name": "Investors",
          "properties": {
            "year": 2025,
            "keyStats": { "items": [
              { "content": { "properties": { "statValue": "130.9m", "statLabel": "EBITDA", "statIcon": [] } } }
            ] }
          }
        }
        """;

        var overview = UmbracoMapper.MapToInvestorOverview(json);

        Assert.Equal(2025, overview.Year);
        Assert.Single(overview.KeyStats);
        Assert.Equal("EBITDA", overview.KeyStats[0].Label);
    }

    [Fact]
    public void MapToInvestorOverview_MissingYear_DefaultsToZero()
    {
        var json = """{ "name": "Investors", "properties": {} }""";

        var overview = UmbracoMapper.MapToInvestorOverview(json);

        Assert.Equal(0, overview.Year);
    }

    // ---- MapToServiceList ----

    private static string ServiceItem(string name, string brand) => $$"""
    {
      "id": "{{Guid.NewGuid()}}",
      "name": "{{name}}",
      "contentType": "servicePage",
      "route": { "path": "/services/{{name}}/", "startItem": { "path": "{{brand}}" } },
      "properties": {}
    }
    """;

    [Fact]
    public void MapToServiceList_WithBrandSlug_ReturnsOnlyThatBrandsServices()
    {
        var json = $$"""
        { "items": [
          {{ServiceItem("cardiac", "uniphar-medtech")}},
          {{ServiceItem("distribution", "uniphar-pharma")}}
        ] }
        """;

        var services = UmbracoMapper.MapToServiceList(json, "uniphar-medtech");

        Assert.Single(services);
        Assert.Equal("cardiac", services[0].Title);
    }

    [Fact]
    public void MapToServiceList_NoItemsProperty_ReturnsEmptyList()
    {
        var services = UmbracoMapper.MapToServiceList("{}");

        Assert.Empty(services);
    }
}