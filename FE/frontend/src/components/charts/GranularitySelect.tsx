import type { Granularity } from '@/types/statistics.types';

interface GranularitySelectProps {
  value:     Granularity;
  onChange:  (g: Granularity) => void;
  options?:  Granularity[];
  ariaLabel?: string;
}

const LABELS: Record<Granularity, string> = {
  day:   'Zilnic',
  week:  'Săptămânal',
  month: 'Lunar',
  year:  'Anual',
};

const selectClass =
  'rounded-lg border border-slate-300 bg-white/70 px-3 py-1.5 text-sm text-slate-700 outline-none transition focus:ring-2 focus:ring-brand-500 cursor-pointer';

/** Select de granularitate. Implicit zi/saptamana/luna/an; restrange optiunile prin `options`. */
export const GranularitySelect: React.FC<GranularitySelectProps> = ({
  value, onChange, options = ['day', 'week', 'month', 'year'], ariaLabel = 'Granularitate',
}) => (
  <select
    className={selectClass}
    value={value}
    aria-label={ariaLabel}
    onChange={e => onChange(e.target.value as Granularity)}
  >
    {options.map(o => <option key={o} value={o}>{LABELS[o]}</option>)}
  </select>
);
