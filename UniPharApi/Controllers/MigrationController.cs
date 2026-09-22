using Microsoft.AspNetCore.Mvc;
using UniPharApi.Services;
using UniPharApi.Models;

namespace UniPharApi.Controllers;

[ApiController]
[Route("api/blog")]
public class BlogController : ControllerBase
{
    private readonly MigrationService _migrationService;
    private readonly BlogService _blogService;

    public BlogController(MigrationService migrationService, BlogService blogService)
    {
        _migrationService = migrationService;
        _blogService = blogService;
    }

    // GET /api/blog — merged list, new posts first then migrated
    [HttpGet]
    public async Task<IActionResult> GetAllPosts()
    {
        var newPosts = await _blogService.GetNewBlogPosts();
        var migratedPosts = await _migrationService.GetMigratedBlogPosts();

        // Tag source so Angular can show badge
        newPosts.ForEach(p => p.ContentType = "blogPost");
        migratedPosts.ForEach(p => p.ContentType = "migratedBlogPost");

        var merged = newPosts.Concat(migratedPosts).ToList();
        return Ok(merged);
    }

    // GET /api/blog/{slug}?source=new|migrated
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