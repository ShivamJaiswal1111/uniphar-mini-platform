using System.Text.Json;
using UniPharApi.Models;
using System.Net;

namespace UniPharApi.Services;

public class BlogService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    public BlogService(IHttpClientFactory factory, IConfiguration config)
    {
        _httpClientFactory = factory;
        _config = config;
    }

    private string GroupHost()
    {
        var host = _config["UmbracoApi:BrandHosts:uniphar-group"];
        if (string.IsNullOrEmpty(host))
            throw new InvalidOperationException("UmbracoApi:BrandHosts:uniphar-group is not configured");
        return host;
    }

    public async Task<List<PageModel>> GetNewBlogPosts()
    {
        var client = _httpClientFactory.CreateClient("UmbracoClient");
        var request = new HttpRequestMessage(HttpMethod.Get,
            "/umbraco/delivery/api/v2/content?fetch=children:/blog-posts/");
        request.Headers.Host = GroupHost();
        request.Headers.Add("Accept-Language", "en-US");

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var result = new List<PageModel>();
        if (!doc.RootElement.TryGetProperty("items", out var items)
            || items.ValueKind != JsonValueKind.Array)
            return result;

        foreach (var item in items.EnumerateArray())
            result.Add(MapNewBlogPost(item));

        return result;
    }

    public async Task<PageModel?> GetNewBlogPost(string slug)
    {
        var client = _httpClientFactory.CreateClient("UmbracoClient");
        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/umbraco/delivery/api/v2/content/item/blog-posts/{slug}");
        request.Headers.Host = GroupHost();
        request.Headers.Add("Accept-Language", "en-US");

        var response = await client.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();

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

                    heroImageUrl = $"{UmbracoMapper.MediaBaseUrl}/api/media/{trimmed}";
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
            Id = item.GetProperty("id").GetString() ?? string.Empty,
            Title = title ?? slug,
            Slug = slug,
            ContentType = "blogPost",
            HeroHeading = title,
            HeroSubtext = $"By {author} — {publishDate?.Split('T')[0]}",
            HeroImageUrl = heroImageUrl,
            BodyContent = UmbracoMapper.ResolveMediaUrls(bodyMarkup ?? string.Empty),
            SidebarContent = $"<p><strong>Author:</strong> {WebUtility.HtmlEncode(author)}</p>" +
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