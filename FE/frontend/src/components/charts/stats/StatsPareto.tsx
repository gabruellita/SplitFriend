import { useMemo } from 'react';
import {
  ComposedChart, Bar, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend, ReferenceLine,
} from 'recharts';
import { statisticsApi } from '@/api/statisticsApi';
import { useStatistics } from '@/hooks/useStatistics';
import { formatMoney } from '@/utils/format';
import { ChartCard } from '../ChartCard';
import { axisTick, gridStroke, formatPct } from './shared';

interface Props {
  from:         string;
  to:           string;
  currencyCode?: string | null;
}

export const StatsPareto: React.FC<Props> = ({ from, to, currencyCode }) => {
  const { data, isLoading, error, refetch } = useStatistics(
    () => statisticsApi.getPareto(from, to),
    [from, to],
  );

  const rows = useMemo(
    () => (data ?? []).map(d => ({ name: d.categoryName ?? 'Fără categorie', total: d.total, cumulativePct: d.cumulativePct })),
    [data],
  );

  return (
    <ChartCard
      title="Pareto 80/20 (cheltuieli)"
      description="Total pe categorie + procent cumulativ"
      isLoading={isLoading}
      error={error}
      onRetry={refetch}
      isEmpty={rows.length === 0}
    >
      <div className="h-64 w-full">
        <ResponsiveContainer width="100%" height="100%">
          <ComposedChart data={rows} margin={{ top: 8, right: 8, bottom: 0, left: -12 }}>
            <CartesianGrid strokeDasharray="3 3" stroke={gridStroke} vertical={false} />
            <XAxis dataKey="name" tick={axisTick} interval={0} angle={-20} textAnchor="end" height={50} />
            <YAxis yAxisId="money" tick={axisTick} width={48} />
            <YAxis yAxisId="pct" orientation="right" domain={[0, 100]} tick={axisTick} width={40} unit="%" />
            <Tooltip
              formatter={(v, name) => (name === 'Cumulativ' ? formatPct(Number(v)) : formatMoney(Number(v), currencyCode))}
            />
            <Legend />
            <ReferenceLine yAxisId="pct" y={80} stroke="var(--color-expense)" strokeDasharray="4 4" />
            <Bar yAxisId="money" dataKey="total" name="Total" fill="var(--color-brand-500)" radius={[3, 3, 0, 0]} />
            <Line yAxisId="pct" type="monotone" dataKey="cumulativePct" name="Cumulativ" stroke="var(--color-expense)" strokeWidth={2} dot={{ r: 3 }} connectNulls />
          </ComposedChart>
        </ResponsiveContainer>
      </div>
    </ChartCard>
  );
};
