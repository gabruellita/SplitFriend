import { useMemo } from 'react';
import { statisticsApi } from '@/api/statisticsApi';
import { useStatistics } from '@/hooks/useStatistics';
import type { CalendarDay } from '@/types/statistics.types';
import { formatMoney, formatDate, toIsoDate } from '@/utils/format';
import { ChartCard } from './ChartCard';

interface Props {
  from:         string;
  to:           string;
  currencyCode?: string | null;
}

interface Cell { iso: string; count: number; total: number; }

// 5 trepte de intensitate (0 = fără date) — albastru brand.
const LEVEL_COLORS = ['#e2e8f0', 'rgba(37,99,235,0.25)', 'rgba(37,99,235,0.45)', 'rgba(37,99,235,0.7)', 'rgba(37,99,235,1)'];

export const CalendarHeatmap: React.FC<Props> = ({ from, to, currencyCode }) => {
  const { data, isLoading, error, refetch } = useStatistics(
    () => statisticsApi.getCalendar(from, to),
    [from, to],
  );

  const { cells, leadingBlanks, maxTotal } = useMemo(() => {
    const map = new Map<string, CalendarDay>();
    for (const d of data ?? []) map.set(d.day, d);

    const start = new Date(`${from}T00:00:00`);
    const end = new Date(`${to}T00:00:00`);
    const out: Cell[] = [];
    let max = 0;

    if (!Number.isNaN(start.getTime()) && !Number.isNaN(end.getTime())) {
      for (let dt = new Date(start); dt <= end; dt.setDate(dt.getDate() + 1)) {
        const iso = toIsoDate(dt);
        const rec = map.get(iso);
        const total = rec?.total ?? 0;
        out.push({ iso, count: rec?.count ?? 0, total });
        if (total > max) max = total;
      }
    }

    // Offset prima coloana: luni=0 ... duminica=6.
    const firstWeekday = out.length > 0 ? (new Date(`${out[0].iso}T00:00:00`).getDay() + 6) % 7 : 0;
    return { cells: out, leadingBlanks: firstWeekday, maxTotal: max };
  }, [data, from, to]);

  const level = (total: number) =>
    total <= 0 ? 0 : Math.min(4, Math.max(1, Math.ceil((total / maxTotal) * 4)));

  return (
    <ChartCard
      title="Activitate zilnică (heatmap)"
      description="Densitatea tranzacțiilor pe zile"
      isLoading={isLoading}
      error={error}
      onRetry={refetch}
      isEmpty={cells.length === 0}
    >
      <div className="overflow-x-auto pb-2">
        <div
          className="grid grid-flow-col gap-1"
          style={{ gridTemplateRows: 'repeat(7, 14px)' }}
          role="img"
          aria-label="Hartă termică a activității zilnice"
        >
          {Array.from({ length: leadingBlanks }).map((_, i) => (
            <span key={`blank-${i}`} className="h-3.5 w-3.5" aria-hidden="true" />
          ))}
          {cells.map(c => (
            <span
              key={c.iso}
              className="h-3.5 w-3.5 rounded-[3px]"
              style={{ backgroundColor: LEVEL_COLORS[level(c.total)] }}
              title={`${formatDate(c.iso)} · ${c.count} tranzacții · ${formatMoney(c.total, currencyCode)}`}
            />
          ))}
        </div>
      </div>

      <div className="mt-3 flex items-center justify-end gap-1.5 text-xs text-slate-500">
        <span>Mai puțin</span>
        {LEVEL_COLORS.map((c, i) => (
          <span key={i} className="h-3 w-3 rounded-[3px]" style={{ backgroundColor: c }} aria-hidden="true" />
        ))}
        <span>Mai mult</span>
      </div>
    </ChartCard>
  );
};
