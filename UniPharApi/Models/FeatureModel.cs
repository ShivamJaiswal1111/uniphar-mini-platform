namespace UniPharApi.Models;

public class FeatureModel
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public bool IsHighlighted { get; set; }
}