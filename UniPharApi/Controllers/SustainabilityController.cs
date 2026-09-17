using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using UniPharApi.Services;
using UniPharApi.Models;

namespace UniPharApi.Controllers;

[ApiController]
[Route("api/{brandSlug}/sustainability")]
public class SustainabilityController : ControllerBase
{
    private readonly UmbracoService _umbracoService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SustainabilityController> _logger;

    public SustainabilityController(UmbracoService umbracoService, IMemoryCache cache, ILogger<SustainabilityController> logger)
    {
        _umbracoService = umbracoService;
        _cache = cache;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetSustainability(string brandSlug, [FromQuery] string culture = "en-US")
    {
        var cacheKey = $"sustainability:{brandSlug}:{culture}";

        if (_cache.TryGetValue(cacheKey, out SustainabilityModel cached))
        {
            _logger.LogInformation("Cache hit: {Key}", cacheKey);
            return Ok(cached);
        }

        try
        {
            var rawJson = await _umbracoService.GetContentByPath("/sustainability", brandSlug, culture);
            var sustainability = UmbracoMapper.MapToSustainability(rawJson);
            _cache.Set(cacheKey, sustainability, TimeSpan.FromMinutes(10));
            _logger.LogInformation("Cache set: {Key}", cacheKey);
            return Ok(sustainability);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            var rawJson = await _umbracoService.GetContentByPath("/sustainability", brandSlug, "en-US");
            var sustainability = UmbracoMapper.MapToSustainability(rawJson);
            return Ok(sustainability);
        }
    }
}