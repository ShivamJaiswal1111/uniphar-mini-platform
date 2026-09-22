
using UniPharApi.Models;
public class ContactModel
{
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? MapEmbed { get; set; }
    public string? OfficeImageUrl { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? HeroHeading { get; set; }
    public string? HeroSubtext { get; set; }
    public string? HeroImageUrl { get; set; }
    public List<BreadcrumbItem> Breadcrumbs { get; set; } = new();
}