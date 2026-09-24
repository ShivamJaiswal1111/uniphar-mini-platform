using System.Text.Json;
using UniPharApi.Models;

namespace UniPharApi.Services;

public class BlogService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public BlogService(IHttpClientFactory factory)
    {
        _httpClientFactory = factory;
    }

    public async Task<List<PageModel>> GetNewBlogPosts()
    {
        var client = _httpClientFactory.CreateClient("UmbracoClient");
        var request = new HttpRequestMessage(HttpMethod.Get,
            "/umbraco/delivery/api/v2/content?fetch=children:/blog-posts/");
        request.Headers.Add("Host", "uniphargroup.localhost");
        request.Headers.Add("Accept-Language", "en-US");

        var response = await client.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);
        var items = doc.RootElement.GetProperty("items");
        var result = new List<PageModel>();

        foreach (var item in items.EnumerateArray())
            result.Add(MapNewBlogPost(item));

        return result;
    }

    public async Task<PageModel?> GetNewBlogPost(string slug)
    {
        var client = _httpClientFactory.CreateClient("UmbracoClient");
        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/umbraco/delivery/api/v2/content/item/blog-posts/{slug}");
        request.Headers.Add("Host", "uniphargroup.localhost");
        request.Headers.Add("Accept-Language", "en-US");

        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return MapNewBlogPost(doc.RootElement);
    }

    private static PageModel MapNewBlogPost(JsonElement item)
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
            {
                var relativeUrl = urlProp.GetString();
                if (!string.IsNullOrEmpty(relativeUrl))
                {
                    var trimmed = relativeUrl.TrimStart('/');
                    if (trimmed.StartsWith("media/", StringComparison.OrdinalIgnoreCase))
                        trimmed = trimmed["media/".Length..];

                    heroImageUrl = $"http://localhost:5220/api/media/{trimmed}";
                }
            }
        }

        var author = props.TryGetProperty("author", out var a) ? a.GetString() : null;
        var publishDate = props.TryGetProperty("publishDate", out var pd) ? pd.GetString() : null;
        var title = props.TryGetProperty("title", out var t) ? t.GetString()
                    : item.GetProperty("name").GetString();
        var bodyMarkup = props.TryGetProperty("body", out var b)
                         && b.TryGetProperty("markup", out var m)
                         ? m.GetString() : null;

        return new PageModel
        {
            Id = item.GetProperty("id").GetString(),
            Title = title ?? slug,
            Slug = slug,
            ContentType = "blogPost",
            HeroHeading = title,
            HeroSubtext = $"By {author} — {publishDate?.Split('T')[0]}",
            HeroImageUrl = heroImageUrl,
            BodyContent = UmbracoMapper.ResolveMediaUrls(bodyMarkup),
            SidebarContent = $"<p><strong>Author:</strong> {author}</p>" +
                            $"<p><strong>Published:</strong> {publishDate?.Split('T')[0]}</p>",
            MetaTitle = title,
            MetaDescription = null,
            FeaturedSections = new List<NavigationCardModel>(),
            Culture = "en-US",
            Breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = "/" },
                new BreadcrumbItem { Title = "Blog", Url = "/blog" },
                new BreadcrumbItem { Title = title ?? slug, Url = null }
            }
        };
    }
}