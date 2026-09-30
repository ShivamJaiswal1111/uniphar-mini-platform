using System.Net;
using System.Text;
using System.Text.Json;

namespace UniPharApi.Tests;

public class BrandFallbackTests
{
    private static HttpResponseMessage Json(string body) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

    private static string Item(string id, string name, string slug) =>
        $"{{\"id\":\"{id}\",\"name\":\"{name}\"," +
        $"\"route\":{{\"path\":\"/\",\"startItem\":{{\"id\":\"{id}\",\"path\":\"{slug}\"}}}}," +
        "\"properties\":{}}";

    private static string List(params string[] items) =>
        $"{{\"total\":{items.Length},\"items\":[{string.Join(",", items)}]}}";

    private static string Lang(HttpRequestMessage req) =>
        req.Headers.TryGetValues("Accept-Language", out var v) ? v.First() : "";

    [Fact]
    public async Task Brands_InFrench_ReturnsAllThree_WithFrenchOverridingEnglish()
    {
        using var factory = new CustomWebApplicationFactory();
        factory.Umbraco.Responder = req => Lang(req) == "fr-FR"
            ? Json(List(Item("m1", "Uniphar Medtech FR", "uniphar-medtech")))
            : Json(List(
                Item("g1", "Uniphar Group", "uniphar-group"),
                Item("m1", "Uniphar Medtech", "uniphar-medtech"),
                Item("p1", "Uniphar Pharma", "uniphar-pharma")));
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/brands?culture=fr-FR");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var names = JsonDocument.Parse(await response.Content.ReadAsStringAsync())
            .RootElement.EnumerateArray()
            .Select(b => b.GetProperty("name").GetString())
            .ToList();
        Assert.Equal(3, names.Count);
        Assert.Contains("Uniphar Medtech FR", names);
        Assert.DoesNotContain("Uniphar Medtech", names);
        Assert.Contains("Uniphar Group", names);
        Assert.Contains("Uniphar Pharma", names);
    }

    [Fact]
    public async Task Brands_InEnglish_CallsUmbracoOnce()
    {
        using var factory = new CustomWebApplicationFactory();
        factory.Umbraco.Responder = _ => Json(List(Item("g1", "Uniphar Group", "uniphar-group")));
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/brands?culture=en-US");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single(factory.Umbraco.Requests);
    }
}