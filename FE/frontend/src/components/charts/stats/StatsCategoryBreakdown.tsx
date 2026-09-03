import { useMemo, useState } from 'react';
import {
  PieChart, Pie, Cell, ResponsiveContainer, Tooltip,
  BarChart, Bar, XAxis, YAxis, CartesianGrid,
} from 'recharts';
import { statisticsApi } from '@/api/statisticsApi';
import { useStatistics } from '@/hooks/useStatistics';
import type { TransactionKind } from '@/types/finance.types';
import { formatMoney } from '@/utils/format';
import { ChartCard } from '../ChartCard';
import { KindToggle } from '../KindToggle';
import { PALETTE, axisTick, gridStroke } from './shared';

interface Props {
  from:         string;
  to:           string;
  currencyCode?: string | null;
}

interface Slice { name: string; value: number; }

export const StatsCategoryBreakdown: React.FC<Props> = ({ from, to, currencyCode }) => {
  const [kind, setKind] = useState<TransactionKind>('EXPENSE');

  const { data, isLoading, error, refetch } = useStatistics(
    () => statisticsApi.getCategoryBreakdown(from, to, kind),
    [from, to, kind],
  );

  const slices = useMemo<Slice[]>(() => {
    const filtered = (data ?? [])
      .filter(d => d.total > 0)
      .sort((a, b) => b.total - a.total)
      .map(d => ({ name: d.categoryName ?? 'Fără categorie', value: d.total }));
    if (filtered.length <= 6) return filtered;
    const top = filtered.slice(0, 6);
    const rest = filtered.slice(6).reduce((s, x) => s + x.value, 0);
    return [...top, { name: 'Altele', value: rest }];
  }, [data]);

  const total = slices.reduce((s, x) => s + x.value, 0);
  // Regula no-pie-overuse: donut doar pentru putine categorii; altfel bar orizontal.
  const useDonut = slices.length <= 5;

  return (
    <ChartCard
      title="Distribuție pe categorii"
      description="Proporție pe categorieSS"
      controls={<KindToggle value={kind} onChange={setKind} />}
      isLoading={isLoading}
      error={error}
      onRetry={refetch}
      isEmpty={slices.length === 0}
    >
      {useDonut ? (
        <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
          <div className="h-48 w-full sm:w-48">
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie data={slices} dataKey="value" nameKey="name" innerRadius={50} outerRadius={80} paddingAngle={2}>
                  {slices.map((_, i) => <Cell key={i} fill={PALETTE[i % PALETTE.length]} />)}
                </Pie>
                <Tooltip formatter={(v) => formatMoney(Number(v), currencyCode)} />
              </PieChart>
            </ResponsiveContainer>
          </div>
          <ul className="flex-1 space-y-1.5 text-sm">
            {slices.map((s, i) => (
              <li key={s.name} className="flex items-center justify-between gap-3">
                <span className="flex items-center gap-2">
                  <span className="h-2.5 w-2.5 rounded-full" style={{ backgroundColor: PALETTE[i % PALETTE.length] }} aria-hidden="true" />
                  <span className="text-slate-700">{s.name}</span>
                </span>
                <span className="tabular-nums text-slate-500">
                  {total > 0 ? Math.round((s.value / total) * 100) : 0}% · {formatMoney(s.value, currencyCode)}
                </span>
              </li>
            ))}
          </ul>
        </div>
      ) : (
        <div className="h-64 w-full">
          <ResponsiveContainer width="100%" height="100%">
            <BarChart data={slices} layout="vertical" margin={{ top: 4, right: 16, bottom: 0, left: 8 }}>
              <CartesianGrid strokeDasharray="3 3" stroke={gridStroke} horizontal={false} />
              <XAxis type="number" tick={axisTick} />
              <YAxis type="category" dataKey="name" tick={axisTick} width={110} />
              <Tooltip formatter={(v) => formatMoney(Number(v), currencyCode)} />
              <Bar dataKey="value" name="Total" radius={[0, 3, 3, 0]}>
                {slices.map((_, i) => <Cell key={i} fill={PALETTE[i % PALETTE.length]} />)}
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        </div>
      )}
    </ChartCard>
  );
};
