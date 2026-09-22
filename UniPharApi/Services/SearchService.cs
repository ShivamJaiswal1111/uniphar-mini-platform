using System.Text.Json;
using UniPharApi.Models;

namespace UniPharApi.Services;

public class SearchService
{
    private readonly UmbracoService _umbracoService;
    private readonly BlogService _blogService;
    private readonly MigrationService _migrationService;

    public SearchService(
        UmbracoService umbracoService,
        BlogService blogService,
        MigrationService migrationService)
    {
        _umbracoService = umbracoService;
        _blogService = blogService;
        _migrationService = migrationService;
    }

    public async Task<List<PageModel>> Search(string query, string culture = "en-US")
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<PageModel>();

        var term = query.Trim().ToLowerInvariant();
        var results = new List<PageModel>();

        // Blog posts (new + migrated)
        var blogResults = new List<PageModel>();
        blogResults.AddRange(await _blogService.GetNewBlogPosts());
        blogResults.AddRange(await _migrationService.GetMigratedBlogPosts());

        results.AddRange(blogResults.Where(p => MatchesPage(p, term)));

        // All page-type content across all brands
        var contentTypes = new[] {
            "standardPage",
            "servicePage",
            "investorOverviewPage",
            "resultsCentrePage",
            "contactPage",
            "sustainabilityPage",
            "homePage"
        };

        foreach (var contentType in contentTypes)
        {
            try
            {
                var rawJson = await _umbracoService.GetContentByType(contentType, culture);
                Console.WriteLine($"[{contentType}] raw length: {rawJson.Length}");
                if (contentType == "contactPage")
                {
                    Console.WriteLine($"[contactPage] FULL JSON: {rawJson}");
                }

                using var doc = JsonDocument.Parse(rawJson);

                if (doc.RootElement.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in items.EnumerateArray())
                    {
                        var itemJson = item.GetRawText();
                        var page = UmbracoMapper.MapToPage(itemJson, culture);

                        // Pull extra searchable text this content type keeps outside PageModel
                        var extraText = ExtractExtraSearchText(item, contentType);

                        if (MatchesPage(page, term) || (extraText?.ToLowerInvariant().Contains(term) ?? false))
                            results.Add(page);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Search failed for {contentType}: {ex.Message}");
            }
        }

        return results;
    }

    private static bool MatchesPage(PageModel p, string term) =>
        (p.Title?.ToLowerInvariant().Contains(term) ?? false) ||
        (p.BodyContent?.ToLowerInvariant().Contains(term) ?? false) ||
        (p.HeroHeading?.ToLowerInvariant().Contains(term) ?? false);

    private static string? ExtractExtraSearchText(JsonElement item, string contentType)
    {
        if (!item.TryGetProperty("properties", out var props)) return null;

        return contentType switch
        {
            "contactPage" => string.Join(" ",
                GetStr(props, "address"),
                GetStr(props, "phoneNumBer"),
                GetStr(props, "emailAddress")),

            "investorOverviewPage" => GetRte(props, "introduction"),

            "sustainabilityPage" => GetRte(props, "overviewText"),

            "resultsCentrePage" => GetRte(props, "resultsSummary"),

            _ => null
        };
    }

    private static string? GetStr(JsonElement props, string name) =>
        props.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

    private static string? GetRte(JsonElement props, string name) =>
        props.TryGetProperty(name, out var rte) && rte.ValueKind == JsonValueKind.Object &&
        rte.TryGetProperty("markup", out var markup) ? markup.GetString() : null;
}