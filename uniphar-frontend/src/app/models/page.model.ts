export interface BreadcrumbItem {
  title: string;
  url: string | null;
}

export interface NavigationCard {
  heading: string;
  description: string;
  linkUrl: string | null;
  imageUrl: string | null;
}

export interface Page {
  id: string;
  title: string;
  slug: string;
  
  brandSlug: string | null;
  contentType: string | null;
  heroHeading: string;
  heroSubtext: string;
  heroImageUrl: string;
  ctaButtonText: string | null;
  ctaButtonLink: string | null;
  metaTitle: string;
  metaDescription: string;
  bodyContent: string;
  sidebarContent: string | null;
  introductionHeading: string | null;
  introductionText: string | null;
  brandColor: string | null;
  featuredSections: NavigationCard[];
  culture: string;
  breadcrumbs: BreadcrumbItem[];
}