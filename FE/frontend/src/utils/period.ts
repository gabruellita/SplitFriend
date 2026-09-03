import { monthRange } from './format';

export interface PeriodValue {
  from:        string;
  to:          string;
  categoryId?: number;
}

export type Preset = 'thisMonth' | 'lastMonth' | 'custom';

export const presetRange = (preset: Exclude<Preset, 'custom'>): { from: string; to: string } => {
  const now = new Date();
  if (preset === 'thisMonth') return monthRange(now.getFullYear(), now.getMonth());
  return monthRange(now.getFullYear(), now.getMonth() - 1);
};

/** Valoarea initiala recomandata: luna curenta. */
export const defaultPeriod = (): PeriodValue => presetRange('thisMonth');
