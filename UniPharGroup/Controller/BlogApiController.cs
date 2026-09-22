using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Controllers;

namespace UniPharGroup.Controllers;

[ApiController]
[Route("umbraco/api/blog")]
public class BlogApiController : UmbracoApiController
{
    private readonly IContentService _contentService;
    private readonly IConfiguration _config;

    public BlogApiController(
        IContentService contentService,
        IConfiguration config)
    {
        _contentService = contentService;
        _config = config;
    }

    [HttpPost("create")]
    public IActionResult CreateBlogPost([FromBody] CreateBlogPostRequest request)
    {
        try
        {
            // Validate shared secret key
            var apiKey = Request.Headers["X-Api-Key"].ToString();
            if (apiKey != _config["Umbraco:CMS:ManagementApi:ApiKey"])
                return Unauthorized("Invalid API key");

            // Run on background thread to avoid deadlock
            var result = Task.Run(() => CreatePost(request)).GetAwaiter().GetResult();
            return result;
        }
        catch (Exception ex)
        {
            return StatusCode(500, new {
                error = ex.Message,
                inner = ex.InnerException?.Message
            });
        }
    }

    private IActionResult CreatePost(CreateBlogPostRequest request)
    {
        // Find Blog Posts parent node
        var roots = _contentService.GetRootContent().ToList();
        
        IContent? blogListingNode = null;
        foreach (var root in roots)
        {
            var children = _contentService
                .GetPagedChildren(root.Id, 0, 100, out _)
                .ToList();
            
            blogListingNode = children
                .FirstOrDefault(c => c.ContentType.Alias == "blogListing");
            
            if (blogListingNode != null) break;
        }

        if (blogListingNode == null)
            return NotFound("Blog Posts parent node not found");

        // Create new blog post
        var newPost = _contentService.Create(
            request.Title,
            blogListingNode.Id,
            "blogPost"
        );

        newPost.SetValue("title", request.Title);
        newPost.SetValue("author", request.Author);
        newPost.SetValue("publishDate", DateTime.UtcNow);
        newPost.SetValue("body", request.Body);

        _contentService.Save(newPost);

        var publishResult = _contentService.Publish(newPost, new[] { "en-US" });

        if (publishResult.Success)
            return Ok(new {
                message = "Blog post created successfully",
                id = newPost.Id,
                key = newPost.Key
            });

        return BadRequest($"Publish failed: {publishResult.EventMessages}");
    }
}

public class CreateBlogPostRequest
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}