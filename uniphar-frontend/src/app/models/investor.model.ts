import { KeyStat } from './key-stat.model';
import { BreadcrumbItem } from './page.model';

export interface InvestorOverview {
  introduction: string;
  keyStats: KeyStat[];
  annualReportUrl: string;
  presentationUrl: string;
  stockTickerEmbed: string;
  year: number;
  heroHeading: string | null;
  heroSubtext: string | null;
  heroImageUrl: string | null;
  breadcrumbs: BreadcrumbItem[];
}