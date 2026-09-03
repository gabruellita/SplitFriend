import { useState } from 'react';
import {
  BarChart, Bar, Cell, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
} from 'recharts';
import { statisticsApi } from '@/api/statisticsApi';
import { useStatistics } from '@/hooks/useStatistics';
import type { TransactionKind } from '@/types/finance.types';
import type { MoMGranularity } from '@/types/statistics.types';
import { formatMoney } from '@/utils/format';
import { ChartCard } from '../ChartCard';
import { KindToggle } from '../KindToggle';
import { GranularitySelect } from '../GranularitySelect';
import { axisTick, gridStroke, bucketLabel, formatPct } from './shared';

interface Props {
  from:         string;
  to:           string;
  currencyCode?: string | null;
}

export const StatsMoM: React.FC<Props> = ({ from, to, currencyCode }) => {
  const [kind, setKind] = useState<TransactionKind>('EXPENSE');
  const [granularity, setGranularity] = useState<MoMGranularity>('month');

  const { data, isLoading, error, refetch } = useStatistics(
    () => statisticsApi.getMoM(from, to, kind, granularity),
    [from, to, kind, granularity],
  );

  const rows = data ?? [];
  const g = granularity === 'year' ? 'year' : 'month';

  return (
    <ChartCard
      title="Comparație perioadă-la-perioadă (MoM / YoY)"
      description="Variație față de perioada anterioară"
      controls={
        <>
          <KindToggle value={kind} onChange={setKind} />
          <GranularitySelect
            value={granularity}
            onChange={(v) => setGranularity(v as MoMGranularity)}
            options={['month', 'year']}
            ariaLabel="Granularitate MoM"
          />
        </>
      }
      isLoading={isLoading}
      error={error}
      onRetry={refetch}
      isEmpty={rows.length === 0}
    >
      <div className="h-64 w-full">
        <ResponsiveContainer width="100%" height="100%">
          <BarChart data={rows} margin={{ top: 8, right: 8, bottom: 0, left: -12 }}>
            <CartesianGrid strokeDasharray="3 3" stroke={gridStroke} vertical={false} />
            <XAxis dataKey="period" tickFormatter={(v) => bucketLabel(String(v), g)} tick={axisTick} />
            <YAxis tick={axisTick} width={48} />
            <Tooltip
              labelFormatter={(l) => bucketLabel(String(l), g)}
              formatter={(v, _n, item) => {
                const p = item?.payload as { changePct?: number | null } | undefined;
                return [`${formatMoney(Number(v), currencyCode)} (${formatPct(p?.changePct)})`, 'Total'];
              }}
            />
            <Bar dataKey="total" radius={[3, 3, 0, 0]}>
              {rows.map((r, i) => (
                <Cell
                  key={i}
                  fill={(r.changePct ?? 0) >= 0 ? 'var(--color-expense)' : 'var(--color-income)'}
                />
              ))}
            </Bar>
          </BarChart>
        </ResponsiveContainer>
      </div>
      <p className="mt-2 text-xs text-slate-500">
        Bare colorate după direcția variației: creștere = roșu, scădere = verde.
      </p>
    </ChartCard>
  );
};
