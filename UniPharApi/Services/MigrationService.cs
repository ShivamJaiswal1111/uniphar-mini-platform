using System.Text.Json;
using UniPharApi.Models;

namespace UniPharApi.Services;

public class MigrationService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public MigrationService(IHttpClientFactory factory)
    {
        _httpClientFactory = factory;
    }

    public async Task<List<PageModel>> GetMigratedBlogPosts()
    {
        var client = _httpClientFactory.CreateClient("LegacyClient");
        var response = await client.GetAsync(
            "/umbraco/delivery/api/v2/content?fetch=children:/blog-posts/");

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var items = doc.RootElement.GetProperty("items");
        var result = new List<PageModel>();

        foreach (var item in items.EnumerateArray())
            result.Add(MapBlogPost(item));

        return result;
    }

    public async Task<PageModel?> GetMigratedBlogPost(string slug)
    {
        var client = _httpClientFactory.CreateClient("LegacyClient");
        var response = await client.GetAsync(
            $"/umbraco/delivery/api/v2/content/item/blog-posts/{slug}");

        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        return MapBlogPost(doc.RootElement);
    }

    private static PageModel MapBlogPost(JsonElement item)
    {
        var props = item.GetProperty("properties");

        var routePath = item.GetProperty("route")
                            .GetProperty("path")
                            .GetString() ?? "";
        var slug = routePath.Trim('/').Split('/').Last();

        string? heroImageUrl = null;
        if (props.TryGetProperty("featuredImage", out var imgProp)
            && imgProp.ValueKind == JsonValueKind.Array
            && imgProp.GetArrayLength() > 0)
        {
            var firstImg = imgProp[0];
            if (firstImg.TryGetProperty("url", out var urlProp))
                heroImageUrl = "http://localhost:2271" + urlProp.GetString();
        }

        var author = props.TryGetProperty("author", out var a) ? a.GetString() : null;
        var publishDate = props.TryGetProperty("publishDate", out var pd) ? pd.GetString() : null;
        var sidebarContent = $"<p><strong>Author:</strong> {author}</p>" +
                             $"<p><strong>Published:</strong> {publishDate?.Split('T')[0]}</p>";

        var title = props.TryGetProperty("title", out var t) ? t.GetString()
                    : item.GetProperty("name").GetString();
        var bodyMarkup = props.TryGetProperty("body", out var b)
                         && b.TryGetProperty("markup", out var m)
                         ? m.GetString() : null;

        return new PageModel
        {
            Id = "migrated-" + item.GetProperty("id").GetString(),
            Title = title ?? slug,
            Slug = slug,
            ContentType = "migratedBlogPost",
            HeroHeading = title,
            HeroSubtext = $"By {author} — {publishDate?.Split('T')[0]}",
            HeroImageUrl = heroImageUrl,
            BodyContent = UmbracoMapper.ResolveMediaUrls(bodyMarkup),
            SidebarContent = sidebarContent,
            MetaTitle = title,
            MetaDescription = null,
            FeaturedSections = new List<NavigationCardModel>(),
            Culture = "en-US"
        };
    }
}