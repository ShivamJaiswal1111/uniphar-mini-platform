using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using UniPharApi.Models;
using UniPharApi.Services;

namespace UniPharApi.Controllers;

[ApiController]
[Route("api/{brandSlug}/services")]
public class ServiceController : ControllerBase
{
    private readonly UmbracoService _umbracoService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ServiceController> _logger;

    public ServiceController(UmbracoService umbracoService, IMemoryCache cache, ILogger<ServiceController> logger)
    {
        _umbracoService = umbracoService;
        _cache = cache;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetServices(string brandSlug, [FromQuery] string culture = "en-US")
    {
        var cacheKey = $"services:{brandSlug}:{culture}";

        if (_cache.TryGetValue(cacheKey, out List<ServiceModel> cached))
        {
            _logger.LogInformation("Cache hit: {Key}", cacheKey);
            return Ok(cached);
        }

        var rawJson = await _umbracoService.GetContentByType("servicePage", culture);
        var services = UmbracoMapper.MapToServiceList(rawJson, brandSlug);

        _cache.Set(cacheKey, services, TimeSpan.FromMinutes(10));
        _logger.LogInformation("Cache set: {Key}", cacheKey);

        return Ok(services);
    }

    [HttpGet("{serviceSlug}")]
    public async Task<IActionResult> GetService(string brandSlug, string serviceSlug, [FromQuery] string culture = "en-US")
    {
        var cacheKey = $"service:{brandSlug}:{serviceSlug}:{culture}";

        if (_cache.TryGetValue(cacheKey, out ServiceModel cached))
        {
            _logger.LogInformation("Cache hit: {Key}", cacheKey);
            return Ok(cached);
        }

        var rawJson = await _umbracoService.GetContentByPath($"/{serviceSlug}", brandSlug, culture);
        var service = UmbracoMapper.MapToService(rawJson);

        _cache.Set(cacheKey, service, TimeSpan.FromMinutes(10));
        _logger.LogInformation("Cache set: {Key}", cacheKey);

        return Ok(service);
    }
}