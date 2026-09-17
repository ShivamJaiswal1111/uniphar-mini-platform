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
}