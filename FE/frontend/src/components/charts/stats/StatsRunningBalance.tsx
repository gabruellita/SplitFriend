import {
  AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
} from 'recharts';
import { statisticsApi } from '@/api/statisticsApi';
import { useStatistics } from '@/hooks/useStatistics';
import { formatMoney } from '@/utils/format';
import { ChartCard } from '../ChartCard';
import { axisTick, gridStroke, bucketLabel } from './shared';

interface Props {
  from:         string;
  to:           string;
  currencyCode?: string | null;
}

export const StatsRunningBalance: React.FC<Props> = ({ from, to, currencyCode }) => {
  const { data, isLoading, error, refetch } = useStatistics(
    () => statisticsApi.getRunningBalance(from, to),
    [from, to],
  );

  const rows = data ?? [];

  return (
    <ChartCard
      title="Sold cumulativ"
      description="Sold rulant în timp"
      isLoading={isLoading}
      error={error}
      onRetry={refetch}
      isEmpty={rows.length === 0}
    >
      <div className="h-64 w-full">
        <ResponsiveContainer width="100%" height="100%">
          <AreaChart data={rows} margin={{ top: 8, right: 8, bottom: 0, left: -12 }}>
            <defs>
              <linearGradient id="stBalance" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stopColor="var(--color-brand-500)" stopOpacity={0.35} />
                <stop offset="100%" stopColor="var(--color-brand-500)" stopOpacity={0} />
              </linearGradient>
            </defs>
            <CartesianGrid strokeDasharray="3 3" stroke={gridStroke} vertical={false} />
            <XAxis dataKey="day" tickFormatter={(v) => bucketLabel(String(v), 'day')} tick={axisTick} />
            <YAxis tick={axisTick} width={48} />
            <Tooltip
              formatter={(v) => formatMoney(Number(v), currencyCode)}
              labelFormatter={(l) => bucketLabel(String(l), 'day')}
            />
            <Area type="monotone" dataKey="balance" name="Sold" stroke="var(--color-brand-600)" strokeWidth={2} fill="url(#stBalance)" />
          </AreaChart>
        </ResponsiveContainer>
      </div>
    </ChartCard>
  );
};
