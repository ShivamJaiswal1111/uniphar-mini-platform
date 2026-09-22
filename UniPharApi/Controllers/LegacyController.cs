using Microsoft.AspNetCore.Mvc;
using UniPharApi.Models;

namespace UniPharApi.Controllers;

[ApiController]
[Route("api/legacy")]
public class LegacyController : ControllerBase
{
    [HttpGet("{slug}")]
    public IActionResult GetLegacyPage(string slug)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "SampleData", $"legacy-{slug}.json");

        if (!System.IO.File.Exists(path))
            return NotFound($"No legacy sample file found for '{slug}'");

        var rawJson = System.IO.File.ReadAllText(path);
        var page = UmbracoMapper.MapFromLegacySite(rawJson);

        return Ok(page);
    }
}