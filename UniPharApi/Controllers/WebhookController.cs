using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniPharApi.Models;
using UniPharApi.Services;

namespace UniPharApi.Controllers;

[ApiController]
[Route("api/webhook")]
public class WebhookController : ControllerBase
{
    private readonly CacheService _cache;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(CacheService cache, ILogger<WebhookController> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    [HttpPost("content-published")]
    [EnableRateLimiting("webhook")]
    public async Task<IActionResult> ContentPublished([FromBody] WebhookPayload payload)
    {
        _logger.LogInformation(
            "Webhook received — Event: {EventName} | ContentType: {ContentType} | ContentId: {ContentId}",
            payload.EventName,
            payload.ContentTypeAlias,
            payload.ContentId
        );

        // One write invalidates every cached page, brand and culture at once
        var newVersion = await _cache.BumpVersionAsync();

        _logger.LogInformation("Content cache version bumped to {Version}", newVersion);

        return Ok(new { message = "Cache invalidated", version = newVersion });
    }
}