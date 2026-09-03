import { useMemo, useState } from 'react';
import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip } from 'recharts';
import { statisticsApi } from '@/api/statisticsApi';
import { useStatistics } from '@/hooks/useStatistics';
import type { TransactionKind } from '@/types/finance.types';
import { formatMoney } from '@/utils/format';
import { ChartCard } from '../ChartCard';
import { KindToggle } from '../KindToggle';

interface Props {
  from:         string;
  to:           string;
  currencyCode?: string | null;
}

const COLORS = { recurring: '#7c3aed', spontaneous: '#0891b2' };

export const StatsRecurringSplit: React.FC<Props> = ({ from, to, currencyCode }) => {
  const [kind, setKind] = useState<TransactionKind>('EXPENSE');

  const { data, isLoading, error, refetch } = useStatistics(
    () => statisticsApi.getRecurringSplit(from, to, kind),
    [from, to, kind],
  );

  const slices = useMemo(() => {
    const rec = (data ?? []).find(d => d.isRecurring)?.total ?? 0;
    const spo = (data ?? []).find(d => !d.isRecurring)?.total ?? 0;
    return [
      { name: 'Recurente', value: rec, color: COLORS.recurring },
      { name: 'Spontane', value: spo, color: COLORS.spontaneous },
    ].filter(s => s.value > 0);
  }, [data]);

  const total = slices.reduce((s, x) => s + x.value, 0);

  return (
    <ChartCard
      title="Recurente vs spontane"
      description="Proporția tranzacțiilor din șabloane (filtrare NULL pe template_id)"
      controls={<KindToggle value={kind} onChange={setKind} />}
      isLoading={isLoading}
      error={error}
      onRetry={refetch}
      isEmpty={slices.length === 0}
    >
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
        <div className="h-48 w-full sm:w-48">
          <ResponsiveContainer width="100%" height="100%">
            <PieChart>
              <Pie data={slices} dataKey="value" nameKey="name" innerRadius={50} outerRadius={80} paddingAngle={2}>
                {slices.map((s, i) => <Cell key={i} fill={s.color} />)}
              </Pie>
              <Tooltip formatter={(v) => formatMoney(Number(v), currencyCode)} />
            </PieChart>
          </ResponsiveContainer>
        </div>
        <ul className="flex-1 space-y-1.5 text-sm">
          {slices.map(s => (
            <li key={s.name} className="flex items-center justify-between gap-3">
              <span className="flex items-center gap-2">
                <span className="h-2.5 w-2.5 rounded-full" style={{ backgroundColor: s.color }} aria-hidden="true" />
                <span className="text-slate-700">{s.name}</span>
              </span>
              <span className="tabular-nums text-slate-500">
                {total > 0 ? Math.round((s.value / total) * 100) : 0}% · {formatMoney(s.value, currencyCode)}
              </span>
            </li>
          ))}
        </ul>
      </div>
    </ChartCard>
  );
};
