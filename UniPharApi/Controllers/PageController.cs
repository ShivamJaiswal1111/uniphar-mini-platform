using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using UniPharApi.Models;
using UniPharApi.Services;

namespace UniPharApi.Controllers;

[ApiController]
[Route("api/{brandSlug}")]
public class PageController : ControllerBase
{
    private readonly UmbracoService _umbracoService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PageController> _logger;

    public PageController(UmbracoService umbracoService, IMemoryCache cache, ILogger<PageController> logger)
    {
        _umbracoService = umbracoService;
        _cache = cache;
        _logger = logger;
    }

    [HttpGet("page/{slug}")]
    public async Task<IActionResult> GetPage(string brandSlug, string slug, [FromQuery] string culture = "en-US")
    {
        var cacheKey = $"page:{brandSlug}:{slug}:{culture}";

        if (_cache.TryGetValue(cacheKey, out PageModel cached))
        {
            _logger.LogInformation("Cache hit: {Key}", cacheKey);
            return Ok(cached);
        }

        var rawJson = await _umbracoService.GetContentByPath($"/{slug}", brandSlug, culture);
        var page = UmbracoMapper.MapToPage(rawJson, culture);

        _cache.Set(cacheKey, page, TimeSpan.FromMinutes(10));
        _logger.LogInformation("Cache set: {Key}", cacheKey);

        return Ok(page);
    }

    [HttpGet("home")]
    public async Task<IActionResult> GetHome(string brandSlug, [FromQuery] string culture = "en-US")
    {
        var cacheKey = $"page:{brandSlug}:home:{culture}";

        if (_cache.TryGetValue(cacheKey, out PageModel cached))
        {
            _logger.LogInformation("Cache hit: {Key}", cacheKey);
            return Ok(cached);
        }

        var rawJson = await _umbracoService.GetContentByPath("/", brandSlug, culture);
        var page = UmbracoMapper.MapToPage(rawJson, culture);

        _cache.Set(cacheKey, page, TimeSpan.FromMinutes(10));
        _logger.LogInformation("Cache set: {Key}", cacheKey);

        return Ok(page);
    }

    [HttpGet("contact")]
    public async Task<IActionResult> GetContact(string brandSlug, [FromQuery] string culture = "en-US")
    {
        var cacheKey = $"contact:{brandSlug}:{culture}";

        if (_cache.TryGetValue(cacheKey, out ContactModel cached))
        {
            _logger.LogInformation("Cache hit: {Key}", cacheKey);
            return Ok(cached);
        }

        var rawJson = await _umbracoService.GetContentByPath("/contact", brandSlug, culture);
        var contact = UmbracoMapper.MapToContact(rawJson);

        _cache.Set(cacheKey, contact, TimeSpan.FromMinutes(10));
        _logger.LogInformation("Cache set: {Key}", cacheKey);

        return Ok(contact);
    }
}