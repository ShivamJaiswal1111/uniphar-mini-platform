namespace UniPharApi.Models;

public class ServiceModel
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public bool IsFeatured { get; set; }
    public List<FeatureModel> Features { get; set; } = new();
}