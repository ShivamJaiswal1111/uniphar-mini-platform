using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using UniPharApi.Models;
using UniPharApi.Services;

namespace UniPharApi.Controllers;

[ApiController]
[Route("api/investors")]
public class InvestorController : ControllerBase
{
    private readonly UmbracoService _umbracoService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<InvestorController> _logger;

    public InvestorController(UmbracoService umbracoService, IMemoryCache cache, ILogger<InvestorController> logger)
    {
        _umbracoService = umbracoService;
        _cache = cache;
        _logger = logger;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview([FromQuery] string culture = "en-US")
    {
        var cacheKey = $"investors:overview:{culture}";

        if (_cache.TryGetValue(cacheKey, out InvestorOverviewModel cached))
        {
            _logger.LogInformation("Cache hit: {Key}", cacheKey);
            return Ok(cached);
        }

        var rawJson = await _umbracoService.GetContentByPath("/investors", "uniphar-group", culture);
        var overview = UmbracoMapper.MapToInvestorOverview(rawJson);

        _cache.Set(cacheKey, overview, TimeSpan.FromMinutes(10));
        _logger.LogInformation("Cache set: {Key}", cacheKey);

        return Ok(overview);
    }
}