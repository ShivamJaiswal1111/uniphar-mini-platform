using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using UniPharApi.Models;
using UniPharApi.Services;

namespace UniPharApi.Controllers;

[ApiController]
[Route("api/brands")]
public class BrandController : ControllerBase
{
    private readonly UmbracoService _umbracoService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<BrandController> _logger;

    public BrandController(UmbracoService umbracoService, IMemoryCache cache, ILogger<BrandController> logger)
    {
        _umbracoService = umbracoService;
        _cache = cache;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllBrands([FromQuery] string culture = "en-US")
    {
        var cacheKey = $"brands:all:{culture}";

        if (_cache.TryGetValue(cacheKey, out List<BrandModel> cached))
        {
            _logger.LogInformation("Cache hit: {Key}", cacheKey);
            return Ok(cached);
        }

        var content = await _umbracoService.GetContentByType("homePage", culture);
        var brands = UmbracoMapper.MapToBrandList(content, culture);

        _cache.Set(cacheKey, brands, TimeSpan.FromMinutes(10));
        _logger.LogInformation("Cache set: {Key}", cacheKey);

        return Ok(brands);
    }

    [HttpGet("{brandSlug}")]
    public async Task<IActionResult> GetBrand(string brandSlug, [FromQuery] string culture = "en-US")
    {
        var cacheKey = $"brand:{brandSlug}:{culture}";

        if (_cache.TryGetValue(cacheKey, out BrandModel cached))
        {
            _logger.LogInformation("Cache hit: {Key}", cacheKey);
            return Ok(cached);
        }

        var rawJson = await _umbracoService.GetContentByPath("/", brandSlug, culture);
        var brand = UmbracoMapper.MapToBrand(rawJson, culture);

        _cache.Set(cacheKey, brand, TimeSpan.FromMinutes(10));
        _logger.LogInformation("Cache set: {Key}", cacheKey);

        return Ok(brand);
    }
}