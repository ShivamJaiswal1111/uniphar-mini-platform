namespace UniPharApi.Models;

public class PageModel
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;

    // Hero composition
    public string? HeroHeading { get; set; }
    public string? HeroSubtext { get; set; }
    public string? HeroImageUrl { get; set; }
    public string? CtaButtonText { get; set; }
    public string? CtaButtonLink { get; set; }

    // SEO composition
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }

    // Standard Page
    public string? BodyContent { get; set; }
    public string? SidebarContent { get; set; }

    // Home Page only (null/empty on other page types)
    public string? IntroductionHeading { get; set; }
    public string? IntroductionText { get; set; }
    public string? BrandColor { get; set; }
    public List<NavigationCardModel> FeaturedSections { get; set; } = new();

    public string Culture { get; set; } = "en-US";   // ← this is the missing line
}