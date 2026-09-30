using UniPharApi.Services;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using UniPharApi.Models;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);
var publicBaseUrl = builder.Configuration["PublicApi:BaseUrl"];
if (!string.IsNullOrWhiteSpace(publicBaseUrl))
{
    UmbracoMapper.MediaBaseUrl = publicBaseUrl;
}
else if (!builder.Environment.IsDevelopment())
{
    throw new InvalidOperationException(
        "PublicApi:BaseUrl must be configured outside Development (e.g. https://api.yoursite.com).");
}

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "UniPharApi", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your token below (no need to type \"Bearer \" — Swagger adds it automatically)"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .WithHeaders("Content-Type", "Authorization", "Accept-Language")
              .WithMethods("GET", "POST");
    });
});

var umbracoClient = builder.Services.AddHttpClient("UmbracoClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["UmbracoApi:BaseUrl"]!);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

if (builder.Environment.IsDevelopment())
{
    umbracoClient.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });
}

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = builder.Configuration["Redis:InstanceName"];
});

builder.Services.AddRateLimiter(options =>
{
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

    options.AddPolicy("webhook", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
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
builder.Services.AddHealthChecks();

builder.Services.AddScoped<UmbracoService>();
builder.Services.AddScoped<BlogService>();
builder.Services.AddScoped<SearchService>();
builder.Services.AddSingleton<CacheService>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // Only trust forwarded headers from these proxies (plus loopback, the default).
    // Production: set ForwardedHeaders__KnownProxies__0 to the reverse proxy's IP.
    var proxies = builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>()
                  ?? Array.Empty<string>();
    foreach (var proxy in proxies)
        options.KnownProxies.Add(System.Net.IPAddress.Parse(proxy));
    // App Service front-end IPs are not stable, so pinning KnownProxies is not practical there.
    // Only enable this where the app is reachable solely through the platform's proxy.
    if (builder.Configuration.GetValue<bool>("ForwardedHeaders:TrustAllProxies"))
    {
        options.KnownProxies.Clear();
        options.KnownIPNetworks.Clear();
    }
});

var app = builder.Build();

app.UseForwardedHeaders();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (HttpRequestException ex) when (!context.Response.HasStarted)
    {
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("UpstreamErrors");
        var notFound = ex.StatusCode == System.Net.HttpStatusCode.NotFound;

        logger.LogWarning("Umbraco call failed ({Status}): {Message}", ex.StatusCode?.ToString() ?? "unreachable", ex.Message);

        context.Response.StatusCode = notFound ? StatusCodes.Status404NotFound : StatusCodes.Status502BadGateway;
        await context.Response.WriteAsJsonAsync(new
        {
            error = notFound ? "Content not found" : "Content service is unavailable. Please try again shortly."
        });
    }
    catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException
                                           && !context.RequestAborted.IsCancellationRequested
                                           && !context.Response.HasStarted)
    {
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("UpstreamErrors");
        logger.LogWarning("Umbraco call timed out: {Message}", ex.Message);

        context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
        await context.Response.WriteAsJsonAsync(new { error = "Content service timed out. Please try again shortly." });
    }
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

