import { BreadcrumbItem } from "./page.model";

export interface Service {
  id: string;
  title: string;
  slug: string;
  heroHeading: string;
  heroSubtext: string;
  heroImageUrl: string | null;
  description: string;
  iconUrl: string | null;
  isFeatured: boolean;
  features: Feature[];
  breadcrumbs: BreadcrumbItem[];
}

export interface Feature {
  title: string;
  description: string;
  iconUrl: string | null;
  isHighlighted: boolean;
}