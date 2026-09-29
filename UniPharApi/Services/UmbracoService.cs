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

    /// <summary>
    /// Umbraco's list query does not apply language fallback, so items with no
    /// translation vanish. This returns every item from the fallback culture,
    /// replaced by the translated version where one exists (matched by id).
    /// </summary>
    public async Task<string> GetContentByTypeWithFallback(
        string contentType, string culture, string fallbackCulture = "en-US")
    {
        var requested = await GetContentByType(contentType, culture);

        if (string.Equals(culture, fallbackCulture, StringComparison.OrdinalIgnoreCase))
            return requested;

        var fallback = await GetContentByType(contentType, fallbackCulture);
        return MergeItemsById(requested, fallback);
    }

    private static string MergeItemsById(string requestedJson, string fallbackJson)
    {
        var requestedItems = JsonNode.Parse(requestedJson)?["items"]?.AsArray() ?? new JsonArray();
        var fallbackItems = JsonNode.Parse(fallbackJson)?["items"]?.AsArray() ?? new JsonArray();

        // Translated items, keyed by id.
        var translated = new Dictionary<string, JsonNode>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in requestedItems)
        {
            var id = (string?)item?["id"];
            if (id != null && item != null) translated[id] = item;
        }

        var merged = new JsonArray();
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Walk the fallback (English) list: prefer the translated version.
        foreach (var item in fallbackItems)
        {
            if (item == null) continue;
            var id = (string?)item["id"];

            if (id != null && translated.TryGetValue(id, out var better))
            {
                merged.Add(better.DeepClone());
                used.Add(id);
            }
            else
            {
                merged.Add(item.DeepClone());
            }
        }

        // Translated items with no English counterpart still get included.
        foreach (var (id, item) in translated)
        {
            if (!used.Contains(id)) merged.Add(item.DeepClone());
        }

        var result = new JsonObject
        {
            ["total"] = merged.Count,
            ["items"] = merged
        };
        return result.ToJsonString();
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