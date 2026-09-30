using Microsoft.AspNetCore.Mvc;
using UniPharApi.Models;
using UniPharApi.Services;

namespace UniPharApi.Controllers;

[ApiController]
[Route("api/investors")]
public class InvestorController : ControllerBase
{
    private readonly UmbracoService _umbracoService;
    private readonly ILogger<InvestorController> _logger;

    public InvestorController(UmbracoService umbracoService, ILogger<InvestorController> logger)
    {
        _umbracoService = umbracoService;
        _logger = logger;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview([FromQuery] string culture = "en-US")
    {
        try
        {
            var rawJson = await _umbracoService.GetContentByPath("/investors/", "uniphar-group", culture);
            return Ok(UmbracoMapper.MapToInvestorOverview(rawJson));
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            _logger.LogInformation("Investor page missing for {Culture}, falling back to en-US", culture);
            var rawJson = await _umbracoService.GetContentByPath("/investors/", "uniphar-group", "en-US");
            return Ok(UmbracoMapper.MapToInvestorOverview(rawJson));
        }
    }
}