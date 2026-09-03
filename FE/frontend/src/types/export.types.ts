export type ExportMode = 'MONTHS' | 'RANGE';
export type ExportBlock = 'SUMMARY' | 'TREND' | 'CATEGORIES' | 'TRANSACTIONS';
export type ExportGranularity = 'DAILY' | 'WEEKLY' | 'MONTHLY';

export interface ExportReportRequest {
  mode: ExportMode;
  months?: string[];                 // "YYYY-MM"
  range?: { from: string; to: string };
  blocks: ExportBlock[];
  options?: {
    granularity?: ExportGranularity;
    runningBalanceInStatement?: boolean;
    cumulativeTotal?: boolean;
  };
}
