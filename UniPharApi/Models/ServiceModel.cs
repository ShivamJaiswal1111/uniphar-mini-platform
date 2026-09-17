namespace UniPharApi.Models;

public class ServiceModel
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Slug { get; set; }
    public string? HeroHeading { get; set; }
    public string? HeroSubtext { get; set; }
    public string? HeroImageUrl { get; set; }
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public bool IsFeatured { get; set; }
    public List<FeatureModel> Features { get; set; } = new();
}