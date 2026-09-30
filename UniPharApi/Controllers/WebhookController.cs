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
    public async Task<IActionResult> ContentPublished()
    {
        var expectedKey = _config["ManagementApi:ApiKey"];
        var providedKey = Request.Headers["X-Api-Key"].FirstOrDefault();

        if (string.IsNullOrEmpty(expectedKey) || providedKey != expectedKey)
        {
            _logger.LogWarning("Webhook rejected — missing or invalid X-Api-Key");
            return Unauthorized(new { error = "Invalid or missing API key" });
        }

        var eventAlias = Request.Headers["Umb-Webhook-Event"].FirstOrDefault();

        // Umbraco sends the content item as the body. Log identifiers only; the body is
        // informational, so an unreadable body must never stop the cache from being cleared.
        string? contentId = null;
        string? contentType = null;
        try
        {
            using var doc = await System.Text.Json.JsonDocument.ParseAsync(Request.Body);
            if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                if (doc.RootElement.TryGetProperty("id", out var id)
                    && id.ValueKind == System.Text.Json.JsonValueKind.String)
                    contentId = id.GetString();

                if (doc.RootElement.TryGetProperty("contentType", out var ct)
                    && ct.ValueKind == System.Text.Json.JsonValueKind.String)
                    contentType = ct.GetString();
            }
        }
        catch (System.Text.Json.JsonException)
        {
            // empty or non-JSON body: fine
        }

        _logger.LogInformation(
            "Webhook received — Event: {Event} | ContentType: {ContentType} | ContentId: {ContentId}",
            eventAlias, contentType, contentId);

        var newVersion = await _cache.BumpVersionAsync();
        _logger.LogInformation("Content cache version bumped to {Version}", newVersion);

        return Ok(new { message = "Cache invalidated", version = newVersion });
    }
}