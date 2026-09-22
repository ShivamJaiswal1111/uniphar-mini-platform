using System.Text.Json;

namespace UniPharApi.Models;

public static class UmbracoMapper
{
    private static readonly string UmbracoBaseUrl = "https://localhost:44335";

    public static string ResolveMediaUrls(string html)
    {
        if (string.IsNullOrEmpty(html)) return html;

        // Rewrites src="/media/xxxxx/file.jpg" to src="http://localhost:5220/api/media/xxxxx/file.jpg"
        return System.Text.RegularExpressions.Regex.Replace(
            html,
            @"src=""/media/([^""]*)""",
            m => $"src=\"http://localhost:5220/api/media/{m.Groups[1].Value}\""
        );
    }
    public static BrandModel MapToBrand(string rawJson, string requestedCulture)
    {
        using var doc = JsonDocument.Parse(rawJson);
        var root = doc.RootElement;
        var props = root.GetProperty("properties");

        return new BrandModel
        {
            Id = root.GetProperty("id").GetString() ?? string.Empty,
            Name = root.GetProperty("name").GetString() ?? string.Empty,
            Slug = ExtractSlug(root),
            LogoUrl = ExtractFirstMediaUrl(props, "ogImage"),
            PrimaryColor = GetStringOrNull(props, "brandColor"),
            Culture = requestedCulture
        };
    }

    public static List<BrandModel> MapToBrandList(string rawJson, string culture = "en-US")
    {
        using var doc = JsonDocument.Parse(rawJson);
        var root = doc.RootElement;
        var results = new List<BrandModel>();

        if (!root.TryGetProperty("items", out var items) || items.ValueKind != JsonValueKind.Array)
            return results;

        foreach (var item in items.EnumerateArray())
        {
            var props = item.GetProperty("properties");

            results.Add(new BrandModel
            {
                Id = item.GetProperty("id").GetString() ?? string.Empty,
                Name = item.GetProperty("name").GetString() ?? string.Empty,
                Slug = ExtractSlug(item),
                LogoUrl = ExtractFirstMediaUrl(props, "ogImage"),
                PrimaryColor = GetStringOrNull(props, "brandColor"),
                Culture = culture
            });
        }

        return results;
    }

    public static PageModel MapToPage(string rawJson, string requestedCulture)
    {
        using var doc = JsonDocument.Parse(rawJson);
        var root = doc.RootElement;
        var props = root.GetProperty("properties");

        return new PageModel
        {
            Id = root.GetProperty("id").GetString() ?? string.Empty,
            Title = root.GetProperty("name").GetString() ?? string.Empty,
            Slug = ExtractSlug(root),
            ContentType = root.GetProperty("contentType").GetString() ?? string.Empty,

            HeroHeading = GetStringOrNull(props, "heroHeading"),
            HeroSubtext = GetStringOrNull(props, "heroSubtext"),
            HeroImageUrl = ExtractFirstMediaUrl(props, "heroImage"),
            CtaButtonText = GetStringOrNull(props, "ctaButtonText"),
            CtaButtonLink = ExtractLinkUrl(props, "ctaButtonLink"),

            MetaTitle = GetStringOrNull(props, "metaTitle"),
            MetaDescription = GetStringOrNull(props, "metaDescription"),

            BodyContent = ExtractRichText(props, "bodyContent"),
            SidebarContent = ExtractRichText(props, "sidebarContent"),

            IntroductionHeading = GetStringOrNull(props, "introductionHeading"),
            IntroductionText = ExtractRichText(props, "introductionText"),
            BrandColor = GetStringOrNull(props, "brandColor"),
            FeaturedSections = MapNavigationCards(props, "featuredSections"),   

            BrandSlug = ExtractBrandSlug(root),
            Culture = requestedCulture,
            Breadcrumbs = BuildBreadcrumbs(root, root.GetProperty("name").GetString() ?? string.Empty)
            
            
        };
        
    }

    public static PageModel MapFromLegacySite(string rawJson)
    {
        using var doc = JsonDocument.Parse(rawJson);
        var root = doc.RootElement;

        return new PageModel
        {
            Id = "legacy-" + Guid.NewGuid().ToString("N")[..8], // legacy source has no real id
            Title = root.GetProperty("page_title").GetString() ?? string.Empty,
            Slug = "about", // hardcoded for this one sample file, for now
            ContentType = "legacyPage",

            BodyContent = root.TryGetProperty("page_body", out var body) ? body.GetString() : null,
            MetaTitle = root.TryGetProperty("seo_title", out var seoTitle) ? seoTitle.GetString() : null,
            MetaDescription = root.TryGetProperty("seo_description", out var seoDesc) ? seoDesc.GetString() : null,

            // Everything Umbraco has that this legacy source doesn't — left null/default, same as any page missing optional fields
            HeroHeading = null,
            HeroSubtext = null,
            HeroImageUrl = null,
            SidebarContent = null,
            FeaturedSections = new List<NavigationCardModel>(),

            Culture = "en-US"
        };
    }

    public static List<NavigationCardModel> MapNavigationCards(JsonElement props, string propertyName)
    {
        var results = new List<NavigationCardModel>();

        foreach (var itemProps in ExtractBlockListItemProperties(props, propertyName))
        {
            results.Add(new NavigationCardModel
            {
                Heading = GetStringOrNull(itemProps, "cardHeading") ?? string.Empty,
                Description = GetStringOrNull(itemProps, "cardDescription"),
                LinkUrl = ExtractLinkUrl(itemProps, "cardLink"),
                ImageUrl = ExtractFirstMediaUrl(itemProps, "cardImage")
            });
        }

        return results;
    }

    public static List<KeyStatModel> MapKeyStats(JsonElement props, string propertyName)
    {
        var results = new List<KeyStatModel>();

        foreach (var itemProps in ExtractBlockListItemProperties(props, propertyName))
        {
            results.Add(new KeyStatModel
            {
                Value = GetStringOrNull(itemProps, "statValue") ?? string.Empty,
                Label = GetStringOrNull(itemProps, "statLabel") ?? string.Empty,
                IconUrl = ExtractFirstMediaUrl(itemProps, "statIcon")
            });
        }

        return results;
    }

    public static InvestorOverviewModel MapToInvestorOverview(string rawJson)
    {
        using var doc = JsonDocument.Parse(rawJson);
        var root = doc.RootElement;
        var props = root.GetProperty("properties");

        return new InvestorOverviewModel
        {
            HeroHeading = GetStringOrNull(props, "heroHeading"),
            HeroSubtext = GetStringOrNull(props, "heroSubtext"),
            HeroImageUrl = ExtractFirstMediaUrl(props, "heroImage"),
            Introduction = ExtractRichText(props, "introduction"),
            KeyStats = MapKeyStats(props, "keyStats"),
            AnnualReportUrl = ExtractFirstMediaUrl(props, "annualReportPdf"),
            PresentationUrl = ExtractFirstMediaUrl(props, "resultPresentationPdf"),
            StockTickerEmbed = GetStringOrNull(props, "stockTickerEmbed"),
            Year = GetIntOrNull(props, "year") ?? 0,
            Breadcrumbs = BuildBreadcrumbs(root, root.GetProperty("name").GetString() ?? string.Empty)
        };
    }

    public static ServiceModel MapToService(string rawJson)
    {
        using var doc = JsonDocument.Parse(rawJson);
        return MapServiceElement(doc.RootElement);
    }

    public static List<ServiceModel> MapToServiceList(string rawJson, string? brandSlug = null)
    {
        using var doc = JsonDocument.Parse(rawJson);
        var root = doc.RootElement;
        var results = new List<ServiceModel>();

        if (!root.TryGetProperty("items", out var items) || items.ValueKind != JsonValueKind.Array)
            return results;

        foreach (var item in items.EnumerateArray())
        {
            if (brandSlug != null && !BelongsToBrand(item, brandSlug))
                continue;

            results.Add(MapServiceElement(item));
        }

        return results;
    }

    private static bool BelongsToBrand(JsonElement element, string brandSlug)
    {
        if (!element.TryGetProperty("route", out var route)) return false;
        if (!route.TryGetProperty("startItem", out var startItem)) return false;
        if (!startItem.TryGetProperty("path", out var path)) return false;
        return string.Equals(path.GetString(), brandSlug, StringComparison.OrdinalIgnoreCase);
    }

    private static ServiceModel MapServiceElement(JsonElement element)
    {
        var props = element.GetProperty("properties");

        return new ServiceModel
        {
            Id = element.GetProperty("id").GetString() ?? string.Empty,
            Title = element.GetProperty("name").GetString() ?? string.Empty,
            Slug = ExtractSlug(element),
            BrandSlug = ExtractBrandSlug(element),
            HeroHeading = GetStringOrNull(props, "heroHeading"),
            HeroSubtext = GetStringOrNull(props, "heroSubtext"),
            HeroImageUrl = ExtractFirstMediaUrl(props, "heroImage"),
            Description = ExtractRichText(props, "serviceDescription"),
            IconUrl = ExtractFirstMediaUrl(props, "serviceIcon"),
            IsFeatured = GetBoolOrDefault(props, "isFeaturedService"),
            Features = MapFeatures(props, "keyFeatured"),
            
            Breadcrumbs = BuildBreadcrumbs(element, element.GetProperty("name").GetString() ?? string.Empty)
        };
    }

    public static List<FeatureModel> MapFeatures(JsonElement props, string propertyName)
    {
        var results = new List<FeatureModel>();

        foreach (var itemProps in ExtractBlockListItemProperties(props, propertyName))
        {
            results.Add(new FeatureModel
            {
                Title = GetStringOrNull(itemProps, "featureTitle") ?? string.Empty,
                Description = GetStringOrNull(itemProps, "featureDescription"),
                IconUrl = ExtractFirstMediaUrl(itemProps, "featureIcon"),
                IsHighlighted = GetBoolOrDefault(itemProps, "isHighlighted")
            });
        }

        return results;
    }

    public static ContactModel MapToContact(string rawJson)
    {
        using var doc = JsonDocument.Parse(rawJson);
        var root = doc.RootElement;
        var props = root.GetProperty("properties");

        return new ContactModel
        {
            HeroHeading = GetStringOrNull(props, "heroHeading"),
            HeroSubtext = GetStringOrNull(props, "heroSubtext"),
            HeroImageUrl = ExtractFirstMediaUrl(props, "heroImage"),
            Address = GetStringOrNull(props, "address"),
            Phone = GetStringOrNull(props, "phoneNumBer"),
            Email = GetStringOrNull(props, "emailAddress"),
            MapEmbed = GetStringOrNull(props, "googleMapEmbed"),
            OfficeImageUrl = ExtractFirstMediaUrl(props, "officeImage"),
            Latitude = GetDoubleOrNull(props, "latitude"),
            Longitude = GetDoubleOrNull(props, "longitude"),
            Breadcrumbs = BuildBreadcrumbs(root, root.GetProperty("name").GetString() ?? string.Empty),
        };
    }
    public static SustainabilityModel MapToSustainability(string rawJson)
    {
        using var doc = JsonDocument.Parse(rawJson);
        var root = doc.RootElement;
        var props = root.GetProperty("properties");

        var model = new SustainabilityModel
        {
            HeroHeading = GetStringOrNull(props, "heroHeading") ?? string.Empty,
            HeroSubtext = GetStringOrNull(props, "heroSubtext") ?? string.Empty,
            HeroImageUrl = ExtractFirstMediaUrl(props, "heroImage"),
            OverviewText = ExtractRichText(props, "overviewText") ?? string.Empty,
            EsgReportUrl = ExtractFirstMediaUrl(props, "esgReportPdf")
        };

        foreach (var itemProps in ExtractBlockListItemProperties(props, "sustainabilityGoals"))
        {
            model.Goals.Add(new GoalModel
            {
                Title = GetStringOrNull(itemProps, "goalTitle") ?? string.Empty,
                Description = GetStringOrNull(itemProps, "goalDescription") ?? string.Empty,
                Target = GetStringOrNull(itemProps, "goalTarget") ?? string.Empty,
                Progress = GetIntOrNull(itemProps, "goalProgress") ?? 0
            });
        }

        model.Breadcrumbs = BuildBreadcrumbs(root, root.GetProperty("name").GetString() ?? string.Empty);

        return model;
    }

    // --- Shared helpers, reused by every mapper ---

    private static string? GetStringOrNull(JsonElement props, string propertyName)
    {
        if (!props.TryGetProperty(propertyName, out var value)) return null;
        return value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }

    private static double? GetDoubleOrNull(JsonElement props, string propertyName)
    {
        if (!props.TryGetProperty(propertyName, out var value))
            return null;

        if (value.ValueKind == JsonValueKind.Null || value.ValueKind == JsonValueKind.Undefined)
            return null;

        if (value.ValueKind == JsonValueKind.Number)
            return value.GetDouble();

        // fallback in case it ever comes through as a string
        if (value.ValueKind == JsonValueKind.String && double.TryParse(value.GetString(), out var parsed))
            return parsed;

        return null;
    }


    private static int? GetIntOrNull(JsonElement props, string propertyName)
    {
        if (!props.TryGetProperty(propertyName, out var value)) return null;
        return value.ValueKind == JsonValueKind.Number ? value.GetInt32() : null;
    }
    private static bool GetBoolOrDefault(JsonElement props, string propertyName)
    {
        if (!props.TryGetProperty(propertyName, out var value)) return false;
        return value.ValueKind == JsonValueKind.True;
    }

    private static string? ExtractFirstMediaUrl(JsonElement props, string propertyName)
    {
        if (!props.TryGetProperty(propertyName, out var media)) return null;
        if (media.ValueKind != JsonValueKind.Array || media.GetArrayLength() == 0) return null;
        var relativeUrl = media[0].TryGetProperty("url", out var url) ? url.GetString() : null;
        if (string.IsNullOrEmpty(relativeUrl)) return null;

        // Umbraco returns something like "/media/l0vgpwcx/download.jpg"
        // Strip the leading "/media/" since MediaController re-adds it
        var trimmed = relativeUrl.TrimStart('/');
        if (trimmed.StartsWith("media/", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed["media/".Length..];
        }

        // Absolute URL pointing at our own .NET API — matches Angular's environment.apiBaseUrl
        return $"http://localhost:5220/api/media/{trimmed}";
    }

    private static string? ExtractBrandSlug(JsonElement root)
    {
        if (!root.TryGetProperty("route", out var route)) return null;
        if (!route.TryGetProperty("startItem", out var startItem)) return null;
        if (!startItem.TryGetProperty("path", out var path)) return null;
        return path.GetString();
    }

    private static string? ExtractRichText(JsonElement props, string propertyName)
    {
        if (!props.TryGetProperty(propertyName, out var rte)) return null;
        var markup = rte.ValueKind == JsonValueKind.Object && rte.TryGetProperty("markup", out var m)
            ? m.GetString()
            : null;
        return ResolveMediaUrls(markup);
    }
    private static string? ExtractLinkUrl(JsonElement props, string propertyName)
    {
        if (!props.TryGetProperty(propertyName, out var link)) return null;
        if (link.ValueKind != JsonValueKind.Array || link.GetArrayLength() == 0) return null;

        var first = link[0];

        // External links have a populated "url" field directly
        if (first.TryGetProperty("url", out var url) && url.ValueKind == JsonValueKind.String)
        {
            return url.GetString();
        }

        // Internal content links have url: null, but a nested route.path
        if (first.TryGetProperty("route", out var route) &&
            route.TryGetProperty("path", out var routePath) &&
            routePath.ValueKind == JsonValueKind.String)
        {
            return routePath.GetString();
        }

        return null;
    }
    private static string ExtractSlug(JsonElement root)
    {
        if (!root.TryGetProperty("route", out var route))
            return string.Empty;

        // Get the full path e.g. "/" or "/cardiac-monitoring"
        var fullPath = route.TryGetProperty("path", out var pathProp)
            ? pathProp.GetString() ?? string.Empty
            : string.Empty;

        var trimmed = fullPath.Trim('/');

        // Root node — path is "/" so trimmed is empty
        // Fall back to startItem.path which has the brand slug
        if (string.IsNullOrEmpty(trimmed))
        {
            if (route.TryGetProperty("startItem", out var startItem) &&
                startItem.TryGetProperty("path", out var startPath))
            {
                return startPath.GetString() ?? string.Empty;
            }
            return string.Empty;
        }

        // Child page — take the last segment of the path
        var segments = trimmed.Split('/');
        return segments[^1];
    }

    private static IEnumerable<JsonElement> ExtractBlockListItemProperties(JsonElement props, string propertyName)
    {
        if (!props.TryGetProperty(propertyName, out var blockList)) yield break;
        if (blockList.ValueKind != JsonValueKind.Object) yield break;
        if (!blockList.TryGetProperty("items", out var items)) yield break;
        if (items.ValueKind != JsonValueKind.Array) yield break;

        foreach (var item in items.EnumerateArray())
        {
            if (item.TryGetProperty("content", out var content) &&
                content.TryGetProperty("properties", out var itemProps))
            {
                yield return itemProps;
            }
        }
    }
    private static List<BreadcrumbItem> BuildBreadcrumbs(JsonElement root, string pageTitle)
    {
        var crumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Title = "Home", Url = "/" }
        };

        if (!root.TryGetProperty("route", out var route)) 
            return crumbs;

        // Get brand slug from startItem
        var brandSlug = string.Empty;
        if (route.TryGetProperty("startItem", out var startItem) &&
            startItem.TryGetProperty("path", out var startPath))
        {
            brandSlug = startPath.GetString() ?? string.Empty;
        }

        // Get full path e.g. "/investors/results-centre"
        var fullPath = route.TryGetProperty("path", out var pathProp)
            ? pathProp.GetString() ?? string.Empty
            : string.Empty;

        var trimmed = fullPath.Trim('/');

        // Root node — no intermediate crumbs, just current page
        if (string.IsNullOrEmpty(trimmed))
        {
            crumbs.Add(new BreadcrumbItem { Title = pageTitle, Url = null });
            return crumbs;
        }

        var segments = trimmed.Split('/');
        var accumulated = string.IsNullOrEmpty(brandSlug) ? "" : $"/{brandSlug}";

        // All segments except last are clickable parents
        for (int i = 0; i < segments.Length - 1; i++)
        {
            accumulated += $"/{segments[i]}";
            crumbs.Add(new BreadcrumbItem
            {
                Title = ToTitleCase(segments[i]),
                Url = accumulated
            });
        }

        // Last segment is current page — not clickable
        crumbs.Add(new BreadcrumbItem { Title = pageTitle, Url = null });

        return crumbs;
    }

    private static string ToTitleCase(string slug)
    {
        return System.Globalization.CultureInfo.CurrentCulture
            .TextInfo.ToTitleCase(slug.Replace("-", " "));
    }
}