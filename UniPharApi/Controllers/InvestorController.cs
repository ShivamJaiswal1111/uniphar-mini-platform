using Microsoft.AspNetCore.Mvc;
using UniPharApi.Models;
using UniPharApi.Services;

namespace UniPharApi.Controllers;

[ApiController]
[Route("api/investors")]
public class InvestorController : ControllerBase
{
    private readonly UmbracoService _umbracoService;
    private readonly CacheService _cache;
    private readonly ILogger<InvestorController> _logger;

    public InvestorController(UmbracoService umbracoService, CacheService cache, ILogger<InvestorController> logger)
    {
        _umbracoService = umbracoService;
        _cache = cache;
        _logger = logger;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview([FromQuery] string culture = "en-US")
    {
        var cacheKey = $"investors:overview:{culture}";

        var cached = await _cache.GetAsync(cacheKey);
        if (cached != null)
        {
            _logger.LogInformation("Cache hit: {Key}", cacheKey);
            return Ok(System.Text.Json.JsonSerializer.Deserialize<InvestorOverviewModel>(cached));
        }

        try
        {
            var rawJson = await _umbracoService.GetContentByPath("/investors/", "uniphar-group", culture);
            var overview = UmbracoMapper.MapToInvestorOverview(rawJson);
            await _cache.SetAsync(cacheKey, System.Text.Json.JsonSerializer.Serialize(overview));
            _logger.LogInformation("Cache set: {Key}", cacheKey);
            return Ok(overview);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            var rawJson = await _umbracoService.GetContentByPath("/investors/", "uniphar-group", "en-US");
            return Ok(UmbracoMapper.MapToInvestorOverview(rawJson));
        }
    }
}