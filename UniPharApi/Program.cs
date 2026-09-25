using UniPharApi.Services;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ---- SERVICES ----
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var umbracoClient = builder.Services.AddHttpClient("UmbracoClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["UmbracoApi:BaseUrl"]!);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Accept Umbraco's self-signed dev certificate — development only
if (builder.Environment.IsDevelopment())
{
    umbracoClient.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });
}

builder.Services.AddHttpClient("LegacyClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:2271");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = builder.Configuration["Redis:InstanceName"];
});

builder.Services.AddRateLimiter(options =>
{
    // Applies to every request automatically, in addition to any endpoint policy
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Stricter limit, applied only where [EnableRateLimiting("webhook")] is present
    options.AddPolicy("webhook", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            """{"error": "Too many requests. Please slow down and try again shortly."}""",
            cancellationToken
        );
    };
});
builder.Services.AddScoped<UmbracoService>();
builder.Services.AddScoped<MigrationService>();
builder.Services.AddScoped<BlogService>();
builder.Services.AddScoped<SearchService>();
builder.Services.AddScoped<CacheService>();

var app = builder.Build();

// ---- PIPELINE ----
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");  
app.UseRateLimiter();   
app.MapControllers();

app.Run();