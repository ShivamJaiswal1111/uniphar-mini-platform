using Microsoft.AspNetCore.Mvc;
using UniPharApi.Services;
using UniPharApi.Models;

namespace UniPharApi.Controllers;

[ApiController]
[Route("api/{brandSlug}/sustainability")]
public class SustainabilityController : ControllerBase
{
    private readonly UmbracoService _umbracoService;
    private readonly CacheService _cache;
    private readonly ILogger<SustainabilityController> _logger;

    public SustainabilityController(UmbracoService umbracoService, CacheService cache, ILogger<SustainabilityController> logger)
    {
        _umbracoService = umbracoService;
        _cache = cache;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetSustainability(string brandSlug, [FromQuery] string culture = "en-US")
    {
        var cacheKey = $"sustainability:{brandSlug}:{culture}";

        var cached = await _cache.GetAsync(cacheKey);
        if (cached != null)
        {
            _logger.LogInformation("Cache hit: {Key}", cacheKey);
            return Ok(System.Text.Json.JsonSerializer.Deserialize<SustainabilityModel>(cached));
        }

        try
        {
            var rawJson = await _umbracoService.GetContentByPath("/sustainability", brandSlug, culture);
            var sustainability = UmbracoMapper.MapToSustainability(rawJson);
            await _cache.SetAsync(cacheKey, System.Text.Json.JsonSerializer.Serialize(sustainability));
            _logger.LogInformation("Cache set: {Key}", cacheKey);
            return Ok(sustainability);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            var rawJson = await _umbracoService.GetContentByPath("/sustainability", brandSlug, "en-US");
            return Ok(UmbracoMapper.MapToSustainability(rawJson));
        }
    }
}