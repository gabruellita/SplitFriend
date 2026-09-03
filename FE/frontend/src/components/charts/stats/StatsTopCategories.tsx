import { useMemo, useState } from 'react';
import {
  BarChart, Bar, Cell, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
} from 'recharts';
import { statisticsApi } from '@/api/statisticsApi';
import { useStatistics } from '@/hooks/useStatistics';
import type { TransactionKind } from '@/types/finance.types';
import { formatMoney } from '@/utils/format';
import { ChartCard } from '../ChartCard';
import { KindToggle } from '../KindToggle';
import { PALETTE, axisTick, gridStroke, formatPct } from './shared';

interface Props {
  from:         string;
  to:           string;
  currencyCode?: string | null;
}

const LIMITS = [3, 5, 10];

const selectClass =
  'rounded-lg border border-slate-300 bg-white/70 px-3 py-1.5 text-sm text-slate-700 outline-none transition focus:ring-2 focus:ring-brand-500 cursor-pointer';

export const StatsTopCategories: React.FC<Props> = ({ from, to, currencyCode }) => {
  const [kind, setKind] = useState<TransactionKind>('EXPENSE');
  const [limit, setLimit] = useState(5);

  const { data, isLoading, error, refetch } = useStatistics(
    () => statisticsApi.getTopCategories(from, to, kind, limit),
    [from, to, kind, limit],
  );

  const rows = useMemo(
    () => (data ?? []).map(d => ({ name: d.categoryName ?? 'Fără categorie', total: d.total, pct: d.pct })),
    [data],
  );

  return (
    <ChartCard
      title="Top categorii"
      description="Primele N categorii + procent din total"
      controls={
        <>
          <KindToggle value={kind} onChange={setKind} />
          <select className={selectClass} value={limit} aria-label="Număr categorii"
            onChange={e => setLimit(Number(e.target.value))}>
            {LIMITS.map(n => <option key={n} value={n}>Top {n}</option>)}
          </select>
        </>
      }
      isLoading={isLoading}
      error={error}
      onRetry={refetch}
      isEmpty={rows.length === 0}
    >
      <div className="h-64 w-full">
        <ResponsiveContainer width="100%" height="100%">
          <BarChart data={rows} layout="vertical" margin={{ top: 4, right: 16, bottom: 0, left: 8 }}>
            <CartesianGrid strokeDasharray="3 3" stroke={gridStroke} horizontal={false} />
            <XAxis type="number" tick={axisTick} />
            <YAxis type="category" dataKey="name" tick={axisTick} width={110} />
            <Tooltip
              formatter={(v, _n, item) => {
                const p = item?.payload as { pct?: number | null } | undefined;
                return [`${formatMoney(Number(v), currencyCode)} (${formatPct(p?.pct)})`, 'Total'];
              }}
            />
            <Bar dataKey="total" name="Total" radius={[0, 3, 3, 0]}>
              {rows.map((_, i) => <Cell key={i} fill={PALETTE[i % PALETTE.length]} />)}
            </Bar>
          </BarChart>
        </ResponsiveContainer>
      </div>
    </ChartCard>
  );
};
