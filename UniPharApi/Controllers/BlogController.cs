using Microsoft.AspNetCore.Mvc;
using UniPharApi.Services;
using System.Text.Json;

namespace UniPharApi.Controllers;

[ApiController]
[Route("api/blog")]
public class BlogController : ControllerBase
{
    private readonly BlogService _blogService;
    private readonly CacheService _cache;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public BlogController(BlogService blogService, CacheService cache)
    {
        _blogService = blogService;
        _cache = cache;
    }

    // GET /api/blog
    [HttpGet]
    public async Task<IActionResult> GetAllPosts()
    {
        var version = await _cache.GetVersionAsync();
        var cacheKey = $"content:{version}:blog:all";

        var cached = await _cache.GetAsync(cacheKey);
        if (cached != null)
        {
            Console.WriteLine($"[CACHE HIT]  {cacheKey}");
            return Content(cached, "application/json");
        }

        Console.WriteLine($"[CACHE MISS] {cacheKey}");

        var posts = await _blogService.GetNewBlogPosts();
        var json = JsonSerializer.Serialize(posts, JsonOptions);

        // Don't cache an empty list: it would be served for 10 minutes even after posts are published.
        if (posts != null && posts.Any())
            await _cache.SetAsync(cacheKey, json, TimeSpan.FromMinutes(10));
        else
            Console.WriteLine($"[CACHE SKIP] {cacheKey} - empty blog list");

        return Content(json, "application/json");
    }

    // GET /api/blog/{slug}
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetPost(string slug)
    {
        var post = await _blogService.GetNewBlogPost(slug);
        if (post == null) return NotFound();
        return Ok(post);
    }
}