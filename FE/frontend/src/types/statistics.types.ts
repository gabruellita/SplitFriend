import type { TransactionKind } from './finance.types';

// ─── Enums / parametri ─────────────────────────────────
export const Granularity = {
  DAY:   'day',
  WEEK:  'week',
  MONTH: 'month',
  YEAR:  'year',
} as const;
export type Granularity = typeof Granularity[keyof typeof Granularity];

/** MoM/YoY accepta doar month/year. */
export type MoMGranularity = 'month' | 'year';

// ─── Response DTOs (camelCase; DateOnly -> "YYYY-MM-DD") ─
export interface TimeseriesPoint {
  bucket: string;
  kind:   TransactionKind;
  total:  number;
}

export interface CategorySlice {
  categoryId:   number | null;
  categoryName: string | null;
  total:        number;
  count:        number;
}

export interface TopCategory {
  categoryName: string | null;
  total:        number;
  pct:          number | null;
}

export interface CalendarDay {
  day:   string;
  count: number;
  total: number;
}

export interface HistogramBucket {
  bucket: number;
  count:  number;
}

export interface SavingsRatePoint {
  month:   string;
  income:  number;
  expense: number;
  rate:    number | null;
}

export interface RunningBalancePoint {
  day:     string;
  balance: number;
}

export interface MoMPoint {
  period:        string;
  total:         number;
  previousTotal: number | null;
  changePct:     number | null;
}

export interface ParetoSlice {
  categoryName:  string | null;
  total:         number;
  cumulativePct: number | null;
}

export interface WeekdayPoint {
  dow:   number;
  day:   string;
  total: number;
  count: number;
}

export interface RecurringSplitPoint {
  isRecurring: boolean;
  total:       number;
  count:       number;
}
