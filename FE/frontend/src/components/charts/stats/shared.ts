import type { Granularity } from '@/types/statistics.types';

/** Paleta accesibila (contrast suficient, distincta fara a depinde doar de culoare). */
export const PALETTE = [
  '#2563eb', '#059669', '#e11d48', '#d97706', '#7c3aed',
  '#0891b2', '#64748b', '#db2777', '#65a30d', '#475569',
];

const MONTHS_RO = ['ian.', 'feb.', 'mar.', 'apr.', 'mai', 'iun.', 'iul.', 'aug.', 'sep.', 'oct.', 'nov.', 'dec.'];

/** Eticheta scurta pentru un bucket ISO ("YYYY-MM-DD"), in functie de granularitate. */
export const bucketLabel = (iso: string, g: Granularity): string => {
  const [y, m, d] = iso.split('-').map(Number);
  const mi = (m ?? 1) - 1;
  switch (g) {
    case 'day':  return `${d} ${MONTHS_RO[mi]}`;
    case 'week': return `${d}/${m}`;
    case 'year': return String(y);
    default:     return `${MONTHS_RO[mi]} ${String(y).slice(2)}`; // month
  }
};

/** Eticheta lunara ("MMM YY") pentru bucket-uri pe luna. */
export const monthLabel = (iso: string): string => bucketLabel(iso, 'month');

export const axisTick = { fontSize: 12, fill: '#64748b' } as const;
export const gridStroke = '#e2e8f0';

/** Procente compacte (ex. "12%"); null -> "—". */
export const formatPct = (v: number | null | undefined): string =>
  v === null || v === undefined ? '—' : `${Math.round(v)}%`;
