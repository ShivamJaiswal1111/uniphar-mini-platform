using System.Net;

namespace UniPharApi.Tests;

public record CapturedRequest(string PathAndQuery, string? Host, string AcceptLanguage, string? StartItem = null);

public class FakeUmbracoHandler : HttpMessageHandler
{
    private readonly List<CapturedRequest> _requests = new();

    public Func<HttpRequestMessage, HttpResponseMessage> Responder { get; set; } =
        _ => new HttpResponseMessage(HttpStatusCode.NotFound);

    public IReadOnlyList<CapturedRequest> Requests
    {
        get { lock (_requests) return _requests.ToList(); }
    }

    public void Reset()
    {
        lock (_requests) _requests.Clear();
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var lang = request.Headers.TryGetValues("Accept-Language", out var values)
            ? string.Join(",", values)
            : "";

        lock (_requests)
            _requests.Add(new CapturedRequest(request.RequestUri!.PathAndQuery, request.Headers.Host, lang,
            request.Headers.TryGetValues("Start-Item", out var startItemValues) ? startItemValues.FirstOrDefault() : null));
        return Task.FromResult(Responder(request));
    }
}