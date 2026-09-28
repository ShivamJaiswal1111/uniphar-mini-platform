using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace UniPharApi.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestApiKey = "test-api-key";
    public const string TestUsername = "test-admin";
    public const string TestPassword = "test-password";

    public FakeUmbracoHandler Umbraco { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ManagementApi:ApiKey", TestApiKey);
        builder.UseSetting("AdminUser:Username", TestUsername);
        builder.UseSetting("AdminUser:Password", TestPassword);
        builder.UseSetting("Jwt:Key", "test-jwt-signing-key-at-least-32-chars-long!");
        builder.UseSetting("Jwt:Issuer", "test-issuer");
        builder.UseSetting("Jwt:Audience", "test-audience");
        builder.UseSetting("Jwt:ExpiryMinutes", "60");
        builder.UseSetting("UmbracoApi:BaseUrl", "https://localhost:1");

        builder.ConfigureServices(services =>
        {
            var redis = services
                .Where(d => d.ServiceType == typeof(IDistributedCache))
                .ToList();
            foreach (var descriptor in redis)
                services.Remove(descriptor);

            services.AddDistributedMemoryCache();

            // Route every call from the "UmbracoClient" named client to the fake
            services.AddHttpClient("UmbracoClient")
                .ConfigurePrimaryHttpMessageHandler(() => Umbraco);
        });
    }
}