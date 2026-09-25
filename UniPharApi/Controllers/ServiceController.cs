using Microsoft.AspNetCore.Mvc;
using UniPharApi.Models;
using UniPharApi.Services;

namespace UniPharApi.Controllers;

[ApiController]
[Route("api/{brandSlug}/services")]
public class ServiceController : ControllerBase
{
    private readonly UmbracoService _umbracoService;
    private readonly CacheService _cache;
    private readonly ILogger<ServiceController> _logger;

    public ServiceController(UmbracoService umbracoService, CacheService cache, ILogger<ServiceController> logger)
    {
        _umbracoService = umbracoService;
        _cache = cache;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetServices(string brandSlug, [FromQuery] string culture = "en-US")
    {
        var cacheKey = $"services:{brandSlug}:{culture}";

        var cached = await _cache.GetAsync(cacheKey);
        if (cached != null)
        {
            _logger.LogInformation("Cache hit: {Key}", cacheKey);
            return Ok(System.Text.Json.JsonSerializer.Deserialize<List<ServiceModel>>(cached));
        }

        var rawJson = await _umbracoService.GetContentByType("servicePage", culture);
        var services = UmbracoMapper.MapToServiceList(rawJson, brandSlug);
        await _cache.SetAsync(cacheKey, System.Text.Json.JsonSerializer.Serialize(services));
        _logger.LogInformation("Cache set: {Key}", cacheKey);

        return Ok(services);
    }

    [HttpGet("{serviceSlug}")]
    public async Task<IActionResult> GetService(string brandSlug, string serviceSlug, [FromQuery] string culture = "en-US")
    {
        var cacheKey = $"service:{brandSlug}:{serviceSlug}:{culture}";

        var cached = await _cache.GetAsync(cacheKey);
        if (cached != null)
        {
            _logger.LogInformation("Cache hit: {Key}", cacheKey);
            return Ok(System.Text.Json.JsonSerializer.Deserialize<ServiceModel>(cached));
        }

        var rawJson = await _umbracoService.GetContentByPath($"/{serviceSlug}", brandSlug, culture);
        var service = UmbracoMapper.MapToService(rawJson);
        await _cache.SetAsync(cacheKey, System.Text.Json.JsonSerializer.Serialize(service));
        _logger.LogInformation("Cache set: {Key}", cacheKey);

        return Ok(service);
    }
}