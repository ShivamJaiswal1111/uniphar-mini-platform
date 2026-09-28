using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace UniPharGroup.Notifications;

public class ContentPublishedWebhookHandler : INotificationHandler<ContentPublishedNotification>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<ContentPublishedWebhookHandler> _logger;

    public ContentPublishedWebhookHandler(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ILogger<ContentPublishedWebhookHandler> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _logger = logger;
    }

    public void Handle(ContentPublishedNotification notification)
    {
        foreach (var entity in notification.PublishedEntities)
        {
            _ = FireWebhookAsync(entity.Id, entity.Name ?? "", entity.ContentType.Alias, "Published");
        }
    }

    private async Task FireWebhookAsync(int contentId, string contentName, string contentTypeAlias, string eventName)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var apiUrl = _config["WebhookTarget:Url"];
            var apiKey = _config["ManagementApi:ApiKey"];

            var payload = new
            {
                EventName = eventName,
                ContentId = contentId.ToString(),
                ContentTypeAlias = contentTypeAlias
            };

            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Add("X-Api-Key", apiKey);

            var response = await client.SendAsync(request);

            _logger.LogInformation(
                "Webhook fired for {ContentName} ({ContentTypeAlias}) — status {StatusCode}",
                contentName, contentTypeAlias, response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Webhook call failed for content ID {ContentId}", contentId);
        }
    }
}