using Microsoft.AspNetCore.Mvc;
using UniPharApi.Models;
using UniPharApi.Services;

namespace UniPharApi.Controllers;

[ApiController]
[Route("api/brands")]
public class BrandController : ControllerBase
{
    private readonly UmbracoService _umbracoService;

    public BrandController(UmbracoService umbracoService)
    {
        _umbracoService = umbracoService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllBrands([FromQuery] string culture = "en-US")
    {
        var raw = await _umbracoService.GetContentByTypeWithFallback("homePage", culture);
        return Ok(UmbracoMapper.MapToBrandList(raw, culture));
    }

    [HttpGet("{brandSlug}")]
    public async Task<IActionResult> GetBrand(string brandSlug, [FromQuery] string culture = "en-US")
    {
        var rawJson = await _umbracoService.GetContentByPath("/", brandSlug, culture);
        return Ok(UmbracoMapper.MapToBrand(rawJson, culture));
    }
}