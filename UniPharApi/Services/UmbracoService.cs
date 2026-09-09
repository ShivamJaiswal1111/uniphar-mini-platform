namespace UniPharApi.Services;

public class UmbracoService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    public UmbracoService(IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

    // Fetches a single content item using a domain-relative path (e.g. "/investors", "/contact", "/")
    // brandSlug determines which brand's hostname is sent as the Host header, so Umbraco
    // resolves the path against the correct root node's domain.
    public async Task<string> GetContentByPath(string domainRelativePath, string brandSlug, string culture = "en-US")
    {
        var client = _httpClientFactory.CreateClient("UmbracoClient");

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/umbraco/delivery/api/v2/content/item{domainRelativePath}");
        request.Headers.Host = ResolveHostname(brandSlug);
        request.Headers.Add("Accept-Language", culture);

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    // Fetches all content items of a given Document Type alias, e.g. "servicePage"
    // Not domain-relative — queries across the whole Umbraco install regardless of brand.
    public async Task<string> GetContentByType(string contentType, string culture = "en-US")
    {
        var client = _httpClientFactory.CreateClient("UmbracoClient");
        client.DefaultRequestHeaders.Remove("Accept-Language");
        client.DefaultRequestHeaders.Add("Accept-Language", culture);

        var response = await client.GetAsync($"/umbraco/delivery/api/v2/content?filter=contentType:{contentType}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    private static string ResolveHostname(string brandSlug) => brandSlug switch
    {
        "uniphar-group" => "uniphargroup.localhost:44335",
        "uniphar-medtech" => "unimedtech.localhost:44335",
        "uniphar-pharma" => "unipharma.localhost:44335",
        _ => "uniphargroup.localhost:44335"
    };

    // Fetches a media file from Umbraco and returns it as a stream
    // mediaPath is the full path e.g. "/media/abc123/hero.jpg"
    public async Task<(Stream stream, string contentType)> GetMediaStream(string mediaPath)
    {
        var client = _httpClientFactory.CreateClient("UmbracoClient");

        var response = await client.GetAsync(mediaPath);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

        return (stream, contentType);
    }
}