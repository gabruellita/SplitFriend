import { useMemo, useState } from 'react';
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
} from 'recharts';
import { statisticsApi } from '@/api/statisticsApi';
import { useStatistics } from '@/hooks/useStatistics';
import { ChartCard } from '../ChartCard';
import { axisTick, gridStroke } from './shared';

interface Props {
  from:         string;
  to:           string;
  currencyCode?: string | null;
}

const MAX_OPTIONS = [500, 1000, 5000];
const BUCKET_OPTIONS = [5, 10, 20];

const selectClass =
  'rounded-lg border border-slate-300 bg-white/70 px-3 py-1.5 text-sm text-slate-700 outline-none transition focus:ring-2 focus:ring-brand-500 cursor-pointer';

export const StatsHistogram: React.FC<Props> = ({ from, to, currencyCode }) => {
  const [max, setMax] = useState(1000);
  const [buckets, setBuckets] = useState(10);

  const { data, isLoading, error, refetch } = useStatistics(
    () => statisticsApi.getHistogram(from, to, max, buckets),
    [from, to, max, buckets],
  );

  // width_bucket() intoarce indexul 1..buckets (+ overflow). Etichetam cu intervalul de sume.
  const rows = useMemo(() => {
    const width = max / buckets;
    return (data ?? []).map(d => {
      const lo = Math.round((d.bucket - 1) * width);
      const hi = d.bucket > buckets ? null : Math.round(d.bucket * width);
      const label = hi === null ? `${Math.round(max)}+` : `${lo}–${hi}`;
      return { label, count: d.count };
    });
  }, [data, max, buckets]);

  return (
    <ChartCard
      title="Distribuția sumelor"
      description={`Număr de tranzacții pe interval de sumă${currencyCode ? ` (${currencyCode})` : ''} (width_bucket())`}
      controls={
        <>
          <select className={selectClass} value={max} aria-label="Sumă maximă"
            onChange={e => setMax(Number(e.target.value))}>
            {MAX_OPTIONS.map(n => <option key={n} value={n}>Max {n}</option>)}
          </select>
          <select className={selectClass} value={buckets} aria-label="Număr intervale"
            onChange={e => setBuckets(Number(e.target.value))}>
            {BUCKET_OPTIONS.map(n => <option key={n} value={n}>{n} intervale</option>)}
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
          <BarChart data={rows} margin={{ top: 8, right: 8, bottom: 0, left: -12 }}>
            <CartesianGrid strokeDasharray="3 3" stroke={gridStroke} vertical={false} />
            <XAxis dataKey="label" tick={axisTick} interval={0} angle={-20} textAnchor="end" height={50} />
            <YAxis allowDecimals={false} tick={axisTick} width={36} />
            <Tooltip formatter={(v) => [`${v} tranzacții`, 'Număr']} />
            <Bar dataKey="count" name="Număr" fill="var(--color-brand-500)" radius={[3, 3, 0, 0]} />
          </BarChart>
        </ResponsiveContainer>
      </div>
    </ChartCard>
  );
};
