using Microsoft.AspNetCore.Mvc;
using UniPharApi.Services;

namespace UniPharApi.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly SearchService _searchService;

    public SearchController(SearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] string culture = "en-US")
    {
        var results = await _searchService.Search(q, culture);
        return Ok(results);
    }
}