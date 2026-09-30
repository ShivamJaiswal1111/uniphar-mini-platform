using System.Net;
using System.Text;
using System.Text.Json;

namespace UniPharApi.Tests;

public class SearchFallbackTests
{
    private static HttpResponseMessage Json(string body) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

    private const string EmptyList = "{\"total\":0,\"items\":[]}";

    private const string WelcomePageList =
        "{\"total\":1,\"items\":[{\"id\":\"s1\",\"contentType\":\"standardPage\",\"name\":\"Welcome Page\"," +
        "\"route\":{\"path\":\"/welcome/\",\"startItem\":{\"id\":\"g1\",\"path\":\"uniphar-group\"}}," +
        "\"properties\":{}}]}";

    private static string Lang(HttpRequestMessage req) =>
        req.Headers.TryGetValues("Accept-Language", out var v) ? v.First() : "";

    [Fact]
    public async Task Search_InFrench_FindsPagesThatOnlyExistInEnglish()
    {
        using var factory = new CustomWebApplicationFactory();
        factory.Umbraco.Responder = req =>
        {
            var query = req.RequestUri?.Query ?? "";

            // Only standardPage has content, and only in en-US.
            // Blog and every other content type return an empty list.
            if (query.Contains("contentType:standardPage") && Lang(req) == "en-US")
                return Json(WelcomePageList);

            return Json(EmptyList);
        };
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/search?q=welcome&culture=fr-FR");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var titles = JsonDocument.Parse(await response.Content.ReadAsStringAsync())
            .RootElement.EnumerateArray()
            .Select(p => p.GetProperty("title").GetString())
            .ToList();

        // Without the fallback, the fr-FR list is empty and this page is never found.
        Assert.Contains("Welcome Page", titles);
    }
}