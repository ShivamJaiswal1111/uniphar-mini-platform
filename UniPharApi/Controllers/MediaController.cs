using Microsoft.AspNetCore.Mvc;
using UniPharApi.Services;

namespace UniPharApi.Controllers;

[ApiController]
[Route("api/media")]
public class MediaController : ControllerBase
{
    private readonly UmbracoService _umbracoService;
    private readonly ILogger<MediaController> _logger;

    public MediaController(UmbracoService umbracoService, ILogger<MediaController> logger)
    {
        _umbracoService = umbracoService;
        _logger = logger;
    }

    [HttpGet("{**mediaPath}")]
    public async Task<IActionResult> GetMedia(string mediaPath)
    {
        try
        {
            var fullPath = $"/media/{mediaPath}";
            _logger.LogInformation("Media requested: {Path}", fullPath);

            var (stream, contentType) = await _umbracoService.GetMediaStream(fullPath);
            return File(stream, contentType);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning("Media not found: {Path} — {Message}", mediaPath, ex.Message);
            return NotFound(new { message = "Media file not found" });
        }
    }
}