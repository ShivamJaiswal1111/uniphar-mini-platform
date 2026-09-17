namespace UniPharApi.Models;

public class InvestorOverviewModel
{
    public string? Introduction { get; set; }
    public List<KeyStatModel> KeyStats { get; set; } = new();
    public string? AnnualReportUrl { get; set; }
    public string? PresentationUrl { get; set; }
    public int Year { get; set; }
    public string? StockTickerEmbed { get; set; }
}