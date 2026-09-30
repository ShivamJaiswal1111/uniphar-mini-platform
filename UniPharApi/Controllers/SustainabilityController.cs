using Microsoft.AspNetCore.Mvc;
using UniPharApi.Services;
using UniPharApi.Models;

namespace UniPharApi.Controllers;

[ApiController]
[Route("api/{brandSlug}/sustainability")]
public class SustainabilityController : ControllerBase
{
    private readonly UmbracoService _umbracoService;
    private readonly ILogger<SustainabilityController> _logger;

    public SustainabilityController(UmbracoService umbracoService, ILogger<SustainabilityController> logger)
    {
        _umbracoService = umbracoService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetSustainability(string brandSlug, [FromQuery] string culture = "en-US")
    {
        try
        {
            var rawJson = await _umbracoService.GetContentByPath("/sustainability", brandSlug, culture);
            return Ok(UmbracoMapper.MapToSustainability(rawJson));
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            _logger.LogInformation("Sustainability page missing for {Brand}/{Culture}, falling back to en-US", brandSlug, culture);
            var rawJson = await _umbracoService.GetContentByPath("/sustainability", brandSlug, "en-US");
            return Ok(UmbracoMapper.MapToSustainability(rawJson));
        }
    }
}