import { useMemo } from 'react';
import {
  AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend,
} from 'recharts';
import { statisticsApi } from '@/api/statisticsApi';
import { useStatistics } from '@/hooks/useStatistics';
import type { Granularity } from '@/types/statistics.types';
import { formatMoney } from '@/utils/format';
import { ChartCard } from '../ChartCard';
import { axisTick, gridStroke, bucketLabel } from './shared';

interface Props {
  from:         string;
  to:           string;
  granularity:  Granularity;
  currencyCode?: string | null;
}

interface Row { bucket: string; income: number; expense: number; }

export const StatsTimeseries: React.FC<Props> = ({ from, to, granularity, currencyCode }) => {
  const { data, isLoading, error, refetch } = useStatistics(
    () => statisticsApi.getTimeseries(from, to, granularity),
    [from, to, granularity],
  );

  const rows = useMemo<Row[]>(() => {
    const byBucket = new Map<string, Row>();
    for (const p of data ?? []) {
      const r = byBucket.get(p.bucket) ?? { bucket: p.bucket, income: 0, expense: 0 };
      if (p.kind === 'INCOME') r.income += p.total;
      else r.expense += p.total;
      byBucket.set(p.bucket, r);
    }
    return [...byBucket.values()].sort((a, b) => a.bucket.localeCompare(b.bucket));
  }, [data]);

  return (
    <ChartCard
      title="Evoluție venituri vs cheltuieli"
      description="Sume agregate pe interval"
      isLoading={isLoading}
      error={error}
      onRetry={refetch}
      isEmpty={rows.length === 0}
    >
      <div className="h-64 w-full">
        <ResponsiveContainer width="100%" height="100%">
          <AreaChart data={rows} margin={{ top: 8, right: 8, bottom: 0, left: -12 }}>
            <defs>
              <linearGradient id="stIncome" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stopColor="var(--color-income)" stopOpacity={0.4} />
                <stop offset="100%" stopColor="var(--color-income)" stopOpacity={0} />
              </linearGradient>
              <linearGradient id="stExpense" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stopColor="var(--color-expense)" stopOpacity={0.4} />
                <stop offset="100%" stopColor="var(--color-expense)" stopOpacity={0} />
              </linearGradient>
            </defs>
            <CartesianGrid strokeDasharray="3 3" stroke={gridStroke} vertical={false} />
            <XAxis dataKey="bucket" tickFormatter={(v) => bucketLabel(String(v), granularity)} tick={axisTick} />
            <YAxis tick={axisTick} width={48} />
            <Tooltip
              formatter={(v) => formatMoney(Number(v), currencyCode)}
              labelFormatter={(l) => bucketLabel(String(l), granularity)}
            />
            <Legend formatter={(v) => (v === 'income' ? 'Venituri' : 'Cheltuieli')} />
            <Area type="monotone" dataKey="income" stroke="var(--color-income)" strokeWidth={2} fill="url(#stIncome)" />
            <Area type="monotone" dataKey="expense" stroke="var(--color-expense)" strokeWidth={2} strokeDasharray="5 3" fill="url(#stExpense)" />
          </AreaChart>
        </ResponsiveContainer>
      </div>
    </ChartCard>
  );
};
