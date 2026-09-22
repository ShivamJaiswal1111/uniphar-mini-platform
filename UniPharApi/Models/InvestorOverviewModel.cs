namespace UniPharApi.Models;

public class InvestorOverviewModel
{
    public string? Introduction { get; set; }
    public List<KeyStatModel> KeyStats { get; set; } = new();
    public string? AnnualReportUrl { get; set; }
    public string? PresentationUrl { get; set; }
    public int Year { get; set; }
    public string? StockTickerEmbed { get; set; }
    public string? HeroHeading { get; set; }
    public string? HeroSubtext { get; set; }
    public string? HeroImageUrl { get; set; }

    public List<BreadcrumbItem> Breadcrumbs { get; set; } = new();
    
}