using Microsoft.AspNetCore.Mvc;
using UniPharApi.Models;
using UniPharApi.Services;

namespace UniPharApi.Controllers;

[ApiController]
[Route("api/{brandSlug}")]
public class PageController : ControllerBase
{
    private readonly UmbracoService _umbracoService;
    private readonly ILogger<PageController> _logger;

    public PageController(UmbracoService umbracoService, ILogger<PageController> logger)
    {
        _umbracoService = umbracoService;
        _logger = logger;
    }

    [HttpGet("page/{**slug}")]
    public async Task<IActionResult> GetPage(string brandSlug, string slug, [FromQuery] string culture = "en-US")
    {
        try
        {
            var rawJson = await _umbracoService.GetContentByPath($"/{slug}", brandSlug, culture);
            return Ok(UmbracoMapper.MapToPage(rawJson, culture));
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            _logger.LogInformation("No {Culture} variant for /{Slug} on {Brand}, falling back to en-US", culture, slug, brandSlug);
            var rawJson = await _umbracoService.GetContentByPath($"/{slug}", brandSlug, "en-US");
            return Ok(UmbracoMapper.MapToPage(rawJson, "en-US"));
        }
    }

    [HttpGet("home")]
    public async Task<IActionResult> GetHome(string brandSlug, [FromQuery] string culture = "en-US")
    {
        try
        {
            var rawJson = await _umbracoService.GetContentByPath("/", brandSlug, culture);
            return Ok(UmbracoMapper.MapToPage(rawJson, culture));
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            _logger.LogInformation("No {Culture} variant for home on {Brand}, falling back to en-US", culture, brandSlug);
            var rawJson = await _umbracoService.GetContentByPath("/", brandSlug, "en-US");
            return Ok(UmbracoMapper.MapToPage(rawJson, "en-US"));
        }
    }

    [HttpGet("contact")]
    public async Task<IActionResult> GetContact(string brandSlug, [FromQuery] string culture = "en-US")
    {
        try
        {
            var rawJson = await _umbracoService.GetContentByPath("/contact", brandSlug, culture);
            return Ok(UmbracoMapper.MapToContact(rawJson));
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            _logger.LogInformation("No {Culture} variant for contact on {Brand}, falling back to en-US", culture, brandSlug);
            var rawJson = await _umbracoService.GetContentByPath("/contact", brandSlug, "en-US");
            return Ok(UmbracoMapper.MapToContact(rawJson));
        }
    }
}