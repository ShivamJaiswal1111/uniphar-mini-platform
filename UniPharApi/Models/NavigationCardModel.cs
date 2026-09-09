namespace UniPharApi.Models;

public class NavigationCardModel
{
    public string Heading { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LinkUrl { get; set; }
    public string? ImageUrl { get; set; }
}