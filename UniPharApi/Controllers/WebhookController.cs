using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using UniPharApi.Models;

namespace UniPharApi.Controllers;

[ApiController]
[Route("api/webhook")]
public class WebhookController : ControllerBase
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(IMemoryCache cache, ILogger<WebhookController> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    [HttpPost("content-published")]
    public IActionResult ContentPublished([FromBody] WebhookPayload payload)
    {
        _logger.LogInformation(
            "Webhook received — Event: {EventName} | ContentType: {ContentType} | ContentId: {ContentId}",
            payload.EventName,
            payload.ContentTypeAlias,
            payload.ContentId
        );

        // Clear all cache by compacting 100% of it
        if (_cache is MemoryCache memoryCache)
        {
            memoryCache.Compact(1.0);
            _logger.LogInformation("Cache cleared after publish event");
        }

        return Ok(new { message = "Webhook received, cache cleared", contentId = payload.ContentId });
    }
}