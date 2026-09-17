namespace UniPharApi.Models;

public class SustainabilityModel
{
    public string HeroHeading { get; set; } = string.Empty;
    public string HeroSubtext { get; set; } = string.Empty;
    public string OverviewText { get; set; } = string.Empty;
    public List<GoalModel> Goals { get; set; } = new();
    public string? EsgReportUrl { get; set; }
}

public class GoalModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public int Progress { get; set; }
}