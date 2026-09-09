import { KeyStat } from './key-stat.model';

export interface InvestorOverview {
  introduction: string;
  keyStats: KeyStat[];
  annualReportUrl: string;
  presentationUrl: string;
  year: number;
}