namespace UniPharApi.Services;
using System.Text.Json.Nodes;

public class UmbracoService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly CacheService _cache;
    private readonly Dictionary<string, string> _brandHosts;

    public UmbracoService(IHttpClientFactory httpClientFactory, CacheService cache, IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _brandHosts = new Dictionary<string, string>(
            config.GetSection("UmbracoApi:BrandHosts").Get<Dictionary<string, string>>() ?? new(),
            StringComparer.OrdinalIgnoreCase);

        if (_brandHosts.Count == 0)
            throw new InvalidOperationException("UmbracoApi:BrandHosts is missing or empty in configuration.");
    }

    public async Task<string> GetContentByPath(string domainRelativePath, string brandSlug, string culture = "en-US")
    {
        var version = await _cache.GetVersionAsync();
        var cacheKey = $"content:{version}:path:{brandSlug}:{domainRelativePath}:{culture}";

        var cached = await _cache.GetAsync(cacheKey);
        if (cached != null)
        {
            Console.WriteLine($"[CACHE HIT]  {cacheKey}");
            return cached;
        }

        Console.WriteLine($"[CACHE MISS] {cacheKey} — calling Umbraco");

        var client = _httpClientFactory.CreateClient("UmbracoClient");
        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/umbraco/delivery/api/v2/content/item{domainRelativePath}");
        request.Headers.Host = ResolveHostname(brandSlug);
        request.Headers.Add("Accept-Language", culture);

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        await _cache.SetAsync(cacheKey, content, TimeSpan.FromMinutes(10));

        return content;
    }

    public async Task<string> GetContentByType(string contentType, string culture = "en-US")
    {
        var version = await _cache.GetVersionAsync();
        var cacheKey = $"content:{version}:type:{contentType}:{culture}";

        var cached = await _cache.GetAsync(cacheKey);
        if (cached != null)
        {
            Console.WriteLine($"[CACHE HIT]  {cacheKey}");
            return cached;
        }

        Console.WriteLine($"[CACHE MISS] {cacheKey} — calling Umbraco");

        var client = _httpClientFactory.CreateClient("UmbracoClient");
        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/umbraco/delivery/api/v2/content?filter=contentType:{contentType}&take=100");
        request.Headers.Add("Accept-Language", culture);

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        await _cache.SetAsync(cacheKey, content, TimeSpan.FromMinutes(10));

        return content;
    }

    // Media is not cached — streams are one-shot and binary, not worth storing in Redis
    public async Task<(Stream stream, string contentType)> GetMediaStream(string mediaPath)
    {
        var client = _httpClientFactory.CreateClient("UmbracoClient");
        var response = await client.GetAsync(mediaPath);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

        return (stream, contentType);
    }


    

    private string ResolveHostname(string brandSlug)
    {
        if (_brandHosts.TryGetValue(brandSlug, out var host))
            return host;

        throw new HttpRequestException(
            $"No hostname configured for brand '{brandSlug}'.",
            null,
            System.Net.HttpStatusCode.NotFound);
    }
}