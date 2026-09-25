using Microsoft.AspNetCore.Mvc;
using UniPharApi.Services;
using UniPharApi.Models;
using System.Text.Json;

namespace UniPharApi.Controllers;

[ApiController]
[Route("api/blog")]
public class BlogController : ControllerBase
{
    private readonly MigrationService _migrationService;
    private readonly BlogService _blogService;
    private readonly CacheService _cache;

    public BlogController(MigrationService migrationService, BlogService blogService, CacheService cache)
    {
        _migrationService = migrationService;
        _blogService = blogService;
        _cache = cache;
    }

    // GET /api/blog — merged list, new posts first then migrated
    [HttpGet]
    public async Task<IActionResult> GetAllPosts()
    {
        var version = await _cache.GetVersionAsync();
        var cacheKey = $"content:{version}:blog:merged";

        var cached = await _cache.GetAsync(cacheKey);
        if (cached != null)
        {
            Console.WriteLine($"[CACHE HIT]  {cacheKey}");
            return Content(cached, "application/json");
        }

        Console.WriteLine($"[CACHE MISS] {cacheKey} — merging blog sources");

        var newPosts = await _blogService.GetNewBlogPosts();
        var migratedPosts = await _migrationService.GetMigratedBlogPosts();

        newPosts.ForEach(p => p.ContentType = "blogPost");
        migratedPosts.ForEach(p => p.ContentType = "migratedBlogPost");

        var merged = newPosts.Concat(migratedPosts).ToList();
        var json = JsonSerializer.Serialize(merged);

        await _cache.SetAsync(cacheKey, json, TimeSpan.FromMinutes(10));

        return Content(json, "application/json");
    }

    // GET /api/blog/{slug}?source=new|migrated  — unchanged, not cached
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetPost(string slug, [FromQuery] string source = "new")
    {
        if (source == "migrated")
        {
            var migrated = await _migrationService.GetMigratedBlogPost(slug);
            if (migrated == null) return NotFound();
            return Ok(migrated);
        }
        else
        {
            var post = await _blogService.GetNewBlogPost(slug);
            if (post == null) return NotFound();
            return Ok(post);
        }
    }
}