using System.Text.Json;
using UniPharApi.Models;

namespace UniPharApi.Services;

public class SearchService
{
    private readonly UmbracoService _umbracoService;
    private readonly BlogService _blogService;
    private readonly MigrationService _migrationService;
    private readonly ILogger<SearchService> _logger;

    public SearchService(
        UmbracoService umbracoService,
        BlogService blogService,
        MigrationService migrationService,
        ILogger<SearchService> logger)
    {
        _umbracoService = umbracoService;
        _blogService = blogService;
        _migrationService = migrationService;
        _logger = logger;
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
        var contentTypes = new[]
        {
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
                using var doc = JsonDocument.Parse(rawJson);

                if (doc.RootElement.TryGetProperty("items", out var items) &&
                    items.ValueKind == JsonValueKind.Array)
                {
                    _logger.LogDebug("Search: {ContentType} returned {Count} items",
                        contentType, items.GetArrayLength());

                    foreach (var item in items.EnumerateArray())
                    {
                        var page = UmbracoMapper.MapToPage(item.GetRawText(), culture);

                        // Extra searchable text this content type keeps outside PageModel
                        var extraText = ExtractExtraSearchText(item, contentType);

                        if (MatchesPage(page, term) ||
                            (extraText?.ToLowerInvariant().Contains(term) ?? false))
                            results.Add(page);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Search failed for content type {ContentType}", contentType);
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