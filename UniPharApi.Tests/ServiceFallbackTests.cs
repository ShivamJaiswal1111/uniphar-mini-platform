using System.Net;
using System.Text;

namespace UniPharApi.Tests;

public class ServiceFallbackTests
{
    private const string EmptyList = "{\"total\":0,\"items\":[]}";
    private const string ListPath = "/umbraco/delivery/api/v2/content?filter=contentType:servicePage&take=100";

    private static HttpResponseMessage Json(string body) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

    [Fact]
    public async Task List_InEnglish_CallsUmbracoOnce()
    {
        using var factory = new CustomWebApplicationFactory();
        factory.Umbraco.Responder = _ => Json(EmptyList);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/uniphar-medtech/services?culture=en-US");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var requests = factory.Umbraco.Requests;
        Assert.Single(requests);
        Assert.Equal("en-US", requests[0].AcceptLanguage);
        Assert.Equal(ListPath, requests[0].PathAndQuery);
    }

    [Fact]
    public async Task List_InFrench_FetchesFrenchThenEnglish()
    {
        using var factory = new CustomWebApplicationFactory();
        factory.Umbraco.Responder = _ => Json(EmptyList);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/uniphar-medtech/services?culture=fr-FR");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var requests = factory.Umbraco.Requests;
        Assert.Equal(2, requests.Count);
        Assert.Equal("fr-FR", requests[0].AcceptLanguage);
        Assert.Equal("en-US", requests[1].AcceptLanguage);
        Assert.All(requests, r => Assert.Equal(ListPath, r.PathAndQuery));
    }

    [Fact]
    public async Task Detail_MissingInFrenchAndEnglish_RetriesOnceThenReturns404()
    {
        using var factory = new CustomWebApplicationFactory();
        factory.Umbraco.Responder = _ => new HttpResponseMessage(HttpStatusCode.NotFound);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/uniphar-medtech/services/does-not-exist?culture=fr-FR");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var requests = factory.Umbraco.Requests;
        Assert.Equal(2, requests.Count);
        Assert.Equal("fr-FR", requests[0].AcceptLanguage);
        Assert.Equal("en-US", requests[1].AcceptLanguage);
        Assert.All(requests, r =>
        {
            Assert.Equal("/umbraco/delivery/api/v2/content/item/does-not-exist", r.PathAndQuery);
            Assert.Equal("uniphar-medtech", r.StartItem);
        });
    }

    [Fact]
    public async Task Detail_WhenUmbracoFails500_DoesNotRetryAndReturns502()
    {
        using var factory = new CustomWebApplicationFactory();
        factory.Umbraco.Responder = _ => new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/uniphar-medtech/services/cardiac-monitoring?culture=fr-FR");

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        Assert.Single(factory.Umbraco.Requests);
    }

    [Fact]
    public async Task Detail_MissingInEnglish_DoesNotRetry()
    {
        using var factory = new CustomWebApplicationFactory();
        factory.Umbraco.Responder = _ => new HttpResponseMessage(HttpStatusCode.NotFound);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/uniphar-medtech/services/does-not-exist?culture=en-US");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Single(factory.Umbraco.Requests);
    }
}