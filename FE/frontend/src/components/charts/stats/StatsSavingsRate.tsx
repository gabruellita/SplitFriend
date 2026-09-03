import {
  ComposedChart, Bar, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend,
} from 'recharts';
import { statisticsApi } from '@/api/statisticsApi';
import { useStatistics } from '@/hooks/useStatistics';
import { formatMoney } from '@/utils/format';
import { ChartCard } from '../ChartCard';
import { axisTick, gridStroke, monthLabel, formatPct } from './shared';

interface Props {
  from:         string;
  to:           string;
  currencyCode?: string | null;
}

export const StatsSavingsRate: React.FC<Props> = ({ from, to, currencyCode }) => {
  const { data, isLoading, error, refetch } = useStatistics(
    () => statisticsApi.getSavingsRate(from, to),
    [from, to],
  );

  const rows = data ?? [];

  return (
    <ChartCard
      title="Rată de economisire"
      description="Venit vs cheltuieli + rata (%) pe lună"
      isLoading={isLoading}
      error={error}
      onRetry={refetch}
      isEmpty={rows.length === 0}
    >
      <div className="h-64 w-full">
        <ResponsiveContainer width="100%" height="100%">
          <ComposedChart data={rows} margin={{ top: 8, right: 8, bottom: 0, left: -12 }}>
            <CartesianGrid strokeDasharray="3 3" stroke={gridStroke} vertical={false} />
            <XAxis dataKey="month" tickFormatter={(v) => monthLabel(String(v))} tick={axisTick} />
            <YAxis yAxisId="money" tick={axisTick} width={48} />
            <YAxis yAxisId="pct" orientation="right" tick={axisTick} width={40} unit="%" />
            <Tooltip
              labelFormatter={(l) => monthLabel(String(l))}
              formatter={(v, name) => (name === 'Rată' ? formatPct(Number(v)) : formatMoney(Number(v), currencyCode))}
            />
            <Legend />
            <Bar yAxisId="money" dataKey="income" name="Venituri" fill="var(--color-income)" radius={[3, 3, 0, 0]} />
            <Bar yAxisId="money" dataKey="expense" name="Cheltuieli" fill="var(--color-expense)" radius={[3, 3, 0, 0]} />
            <Line yAxisId="pct" type="monotone" dataKey="rate" name="Rată" stroke="var(--color-brand-600)" strokeWidth={2} dot={{ r: 3 }} connectNulls />
          </ComposedChart>
        </ResponsiveContainer>
      </div>
    </ChartCard>
  );
};
