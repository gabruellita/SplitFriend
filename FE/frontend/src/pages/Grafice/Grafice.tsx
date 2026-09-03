import { useState } from 'react';

import { PeriodFilter } from '@/components/dashboard/PeriodFilter';
import { GranularitySelect } from '@/components/charts/GranularitySelect';
import { defaultPeriod, type PeriodValue } from '@/utils/period';
import { Granularity } from '@/types/statistics.types';
import { useUserCurrency } from '@/hooks/useUserCurrency';

import { StatsTimeseries } from '@/components/charts/stats/StatsTimeseries';
import { StatsRunningBalance } from '@/components/charts/stats/StatsRunningBalance';
import { StatsSavingsRate } from '@/components/charts/stats/StatsSavingsRate';
import { StatsMoM } from '@/components/charts/stats/StatsMoM';
import { StatsCategoryBreakdown } from '@/components/charts/stats/StatsCategoryBreakdown';
import { StatsTopCategories } from '@/components/charts/stats/StatsTopCategories';
import { StatsPareto } from '@/components/charts/stats/StatsPareto';
import { CalendarHeatmap } from '@/components/charts/CalendarHeatmap';
import { StatsHistogram } from '@/components/charts/stats/StatsHistogram';
import { StatsWeekday } from '@/components/charts/stats/StatsWeekday';
import { StatsRecurringSplit } from '@/components/charts/stats/StatsRecurringSplit';

const Section: React.FC<{ title: string; children: React.ReactNode }> = ({ title, children }) => (
  <section className="space-y-3">
    <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500">{title}</h2>
    <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">{children}</div>
  </section>
);

export const Grafice: React.FC = () => {
  const [period, setPeriod] = useState<PeriodValue>(defaultPeriod());
  const [granularity, setGranularity] = useState<Granularity>('month');

  // Codul real al monedei contului (rezolvat din preferredCurrencyId — vezi useUserCurrency).
  const currencyCode = useUserCurrency();

  const { from, to } = period;
  const common = { from, to, currencyCode };

  return (
    <div className="space-y-6">
      {/* Antet + filtre globale (sticky) */}
      <div className="sticky top-0 z-10 -mx-4 flex flex-wrap items-center justify-between gap-3 bg-slate-50/80 px-4 py-3 backdrop-blur-md lg:-mx-6 lg:px-6">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Grafice</h1>
          <p className="text-sm text-slate-500">Analize vizuale ale finanțelor tale.</p>
        </div>
        <div className="flex flex-wrap items-center gap-2">
          <PeriodFilter value={period} onChange={setPeriod} />
          <GranularitySelect value={granularity} onChange={setGranularity} ariaLabel="Granularitate evoluție" />
        </div>
      </div>

      <Section title="Evoluție în timp">
        <StatsTimeseries {...common} granularity={granularity} />
        <StatsRunningBalance {...common} />
        <StatsSavingsRate {...common} />
        <StatsMoM {...common} />
      </Section>

      <Section title="Categorii">
        <StatsCategoryBreakdown {...common} />
        <StatsTopCategories {...common} />
        <StatsPareto {...common} />
      </Section>

      <Section title="Tipare">
        <CalendarHeatmap {...common} />
        <StatsHistogram {...common} />
        <StatsWeekday {...common} />
        <StatsRecurringSplit {...common} />
      </Section>
    </div>
  );
};
