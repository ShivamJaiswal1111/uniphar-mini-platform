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
    private readonly IConfiguration _config;

    public WebhookController(CacheService cache, ILogger<WebhookController> logger, IConfiguration config)
    {
        _cache = cache;
        _logger = logger;
        _config = config;
    }

    [HttpPost("content-published")]
    [EnableRateLimiting("webhook")]
    public async Task<IActionResult> ContentPublished([FromBody] WebhookPayload payload)
    {
        var expectedKey = _config["ManagementApi:ApiKey"];
        var providedKey = Request.Headers["X-Api-Key"].FirstOrDefault();

        if (string.IsNullOrEmpty(expectedKey) || providedKey != expectedKey)
        {
            _logger.LogWarning("Webhook rejected — missing or invalid X-Api-Key");
            return Unauthorized(new { error = "Invalid or missing API key" });
        }

        _logger.LogInformation(
            "Webhook received — Event: {EventName} | ContentType: {ContentType} | ContentId: {ContentId}",
            payload.EventName,
            payload.ContentTypeAlias,
            payload.ContentId
        );

        var newVersion = await _cache.BumpVersionAsync();

        _logger.LogInformation("Content cache version bumped to {Version}", newVersion);

        return Ok(new { message = "Cache invalidated", version = newVersion });
    }
}