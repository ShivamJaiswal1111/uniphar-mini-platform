export interface Service {
  id: string;
  title: string;
  slug: string;
  description: string;
  iconUrl: string;
  isFeatured: boolean;
  features: string[];
}