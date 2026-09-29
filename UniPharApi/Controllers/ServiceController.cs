using System.Net;
using Microsoft.AspNetCore.Mvc;
using UniPharApi.Models;
using UniPharApi.Services;

namespace UniPharApi.Controllers;

[ApiController]
[Route("api/{brandSlug}/services")]
public class ServiceController : ControllerBase
{
    private const string FallbackCulture = "en-US";

    private readonly UmbracoService _umbracoService;
    private readonly ILogger<ServiceController> _logger;

    public ServiceController(UmbracoService umbracoService, ILogger<ServiceController> logger)
    {
        _umbracoService = umbracoService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetServices(string brandSlug, [FromQuery] string culture = FallbackCulture)
    {
        // Umbraco's list query has no language fallback, so the merge happens in UmbracoService.
        var rawJson = await _umbracoService.GetContentByTypeWithFallback("servicePage", culture, FallbackCulture);
        var services = UmbracoMapper.MapToServiceList(rawJson, brandSlug);
        return Ok(services);
    }

    [HttpGet("{serviceSlug}")]
    public async Task<IActionResult> GetService(string brandSlug, string serviceSlug, [FromQuery] string culture = FallbackCulture)
    {
        string rawJson;
        try
        {
            rawJson = await _umbracoService.GetContentByPath($"/{serviceSlug}", brandSlug, culture);
        }
        catch (HttpRequestException ex) when (
            ex.StatusCode == HttpStatusCode.NotFound &&
            !string.Equals(culture, FallbackCulture, StringComparison.OrdinalIgnoreCase))
        {
            // No variant in the requested culture: serve the same page in English.
            _logger.LogInformation(
                "Service {Slug} has no {Culture} variant for {Brand}, falling back to {Fallback}",
                serviceSlug, culture, brandSlug, FallbackCulture);

            rawJson = await _umbracoService.GetContentByPath($"/{serviceSlug}", brandSlug, FallbackCulture);
        }

        var service = UmbracoMapper.MapToService(rawJson);
        return Ok(service);
    }
}