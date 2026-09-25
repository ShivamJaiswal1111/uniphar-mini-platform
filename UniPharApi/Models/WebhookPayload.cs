
namespace UniPharApi.Models;


public class WebhookPayload
{
    public string EventName { get; set; } = string.Empty;
    public string ContentId { get; set; } = string.Empty;
    public string ContentTypeAlias { get; set; } = string.Empty;
}