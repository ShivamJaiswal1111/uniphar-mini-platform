using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace UniPharApi.Tests;

// An IDistributedCache where every operation fails, like Redis being down
public class ThrowingCache : IDistributedCache
{
    private static InvalidOperationException Down() => new("Simulated Redis outage");

    public byte[]? Get(string key) => throw Down();
    public Task<byte[]?> GetAsync(string key, CancellationToken token = default) => throw Down();
    public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => throw Down();
    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default) => throw Down();
    public void Refresh(string key) => throw Down();
    public Task RefreshAsync(string key, CancellationToken token = default) => throw Down();
    public void Remove(string key) => throw Down();
    public Task RemoveAsync(string key, CancellationToken token = default) => throw Down();
}

// Same setup as CustomWebApplicationFactory, but the cache is the broken one
public class ThrowingCacheFactory : CustomWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices(services =>
        {
            var existing = services
                .Where(d => d.ServiceType == typeof(IDistributedCache))
                .ToList();
            foreach (var descriptor in existing)
                services.Remove(descriptor);

            services.AddSingleton<IDistributedCache, ThrowingCache>();
        });
    }
}

public class CacheFailureTests : IClassFixture<ThrowingCacheFactory>
{
    private readonly ThrowingCacheFactory _factory;
    private readonly HttpClient _client;

    public CacheFailureTests(ThrowingCacheFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Page_WhenCacheThrows_StillReturnsPageFromUmbraco()
    {
        var json = JsonSerializer.Serialize(new
        {
            id = "22222222-2222-2222-2222-222222222222",
            name = "Cache Down",
            contentType = "standardPage",
            route = new { path = "/cache-down/", startItem = new { path = "uniphar-group" } },
            properties = new { heroHeading = "Still here" }
        });

        _factory.Umbraco.Reset();
        _factory.Umbraco.Responder = _ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var response = await _client.GetAsync("/api/uniphar-group/page/cache-down");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}