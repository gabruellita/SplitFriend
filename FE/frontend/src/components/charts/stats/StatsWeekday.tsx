import { useMemo, useState } from 'react';
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
} from 'recharts';
import { statisticsApi } from '@/api/statisticsApi';
import { useStatistics } from '@/hooks/useStatistics';
import type { TransactionKind } from '@/types/finance.types';
import { formatMoney } from '@/utils/format';
import { ChartCard } from '../ChartCard';
import { KindToggle } from '../KindToggle';
import { axisTick, gridStroke } from './shared';

interface Props {
  from:         string;
  to:           string;
  currencyCode?: string | null;
}

const DOW_RO = ['Dum', 'Lun', 'Mar', 'Mie', 'Joi', 'Vin', 'Sâm']; // dow 0..6, 0=duminică

export const StatsWeekday: React.FC<Props> = ({ from, to, currencyCode }) => {
  const [kind, setKind] = useState<TransactionKind>('EXPENSE');

  const { data, isLoading, error, refetch } = useStatistics(
    () => statisticsApi.getWeekday(from, to, kind),
    [from, to, kind],
  );

  // Reordonăm luni→duminică pentru lizibilitate.
  const rows = useMemo(() => {
    const byDow = new Map<number, number>();
    for (const d of data ?? []) byDow.set(d.dow, d.total);
    const order = [1, 2, 3, 4, 5, 6, 0];
    return order.map(dow => ({ label: DOW_RO[dow], total: byDow.get(dow) ?? 0 }));
  }, [data]);

  const hasData = rows.some(r => r.total > 0);
  const barColor = kind === 'INCOME' ? 'var(--color-income)' : 'var(--color-expense)';

  return (
    <ChartCard
      title="Pe ziua săptămânii"
      description="Sume agregate pe zi a săptămânii (EXTRACT(DOW))"
      controls={<KindToggle value={kind} onChange={setKind} />}
      isLoading={isLoading}
      error={error}
      onRetry={refetch}
      isEmpty={!hasData}
    >
      <div className="h-64 w-full">
        <ResponsiveContainer width="100%" height="100%">
          <BarChart data={rows} margin={{ top: 8, right: 8, bottom: 0, left: -12 }}>
            <CartesianGrid strokeDasharray="3 3" stroke={gridStroke} vertical={false} />
            <XAxis dataKey="label" tick={axisTick} />
            <YAxis tick={axisTick} width={48} />
            <Tooltip formatter={(v) => formatMoney(Number(v), currencyCode)} />
            <Bar dataKey="total" name="Total" fill={barColor} radius={[3, 3, 0, 0]} />
          </BarChart>
        </ResponsiveContainer>
      </div>
    </ChartCard>
  );
};
