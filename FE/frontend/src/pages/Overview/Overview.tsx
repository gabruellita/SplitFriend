import { useState } from 'react';
import { TrendingUp, TrendingDown, Wallet } from 'lucide-react';

import { useAuth } from '@/hooks/useAuth';
import { useSummary } from '@/hooks/useSummary';
import { useTransactions } from '@/hooks/useTransactions';
import { useUserCurrency } from '@/hooks/useUserCurrency';

import { Alert } from '@/components/common/Alert';
import { StatCard } from '@/components/dashboard/StatCard';
import { TransactionList } from '@/components/dashboard/TransactionList';
import { PeriodFilter } from '@/components/dashboard/PeriodFilter';
import { defaultPeriod, type PeriodValue } from '@/utils/period';
import { TrendArea } from '@/components/charts/TrendArea';
import { CategoryDonut } from '@/components/charts/CategoryDonut';

export const Overview: React.FC = () => {
  const { user } = useAuth();
  const [period, setPeriod] = useState<PeriodValue>(defaultPeriod());

  const { summary, isLoading: summaryLoading, error: summaryError } = useSummary(period.from, period.to);
  const { transactions, isLoading: txLoading } = useTransactions({ from: period.from, to: period.to });

  const greetingName = user?.firstName ?? user?.username ?? '';
  const currencyCode = useUserCurrency();
  const recent = transactions.slice(0, 5);

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <div className="flex flex-wrap items-center gap-2">
            <h1 className="text-2xl font-bold text-slate-900">
              Salut{greetingName ? `, ${greetingName}` : ''}!
            </h1>
            {currencyCode && (
              <span className="rounded-full bg-slate-100 px-2.5 py-0.5 text-xs font-medium text-slate-600 ring-1 ring-slate-900/5">
                Moneda ta: {currencyCode}
              </span>
            )}
          </div>
          <p className="text-sm text-slate-500">Iată o privire de ansamblu asupra finanțelor tale.</p>
        </div>
        <PeriodFilter value={period} onChange={setPeriod} />
      </div>

      {summaryError && <Alert type="error" message={summaryError} />}

      {/* Carduri sumar */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <StatCard label="Sold net" tone="dark" icon={Wallet}
          amount={summary?.net ?? 0} currencyCode={currencyCode} />
        <StatCard label="Total venituri" tone="income" icon={TrendingUp}
          amount={summary?.totalIncome ?? 0} currencyCode={currencyCode} />
        <StatCard label="Total cheltuieli" tone="expense" icon={TrendingDown}
          amount={summary?.totalExpense ?? 0} currencyCode={currencyCode} />
      </div>

      {/* Grafice */}
      <div className="grid grid-cols-1 gap-4 lg:grid-cols-5">
        <div className="rounded-2xl glass-card p-5 lg:col-span-3">
          <h2 className="mb-4 text-base font-semibold text-slate-900">Evoluție venituri vs cheltuieli</h2>
          {txLoading
            ? <div className="h-64 animate-pulse rounded-xl bg-slate-200/60" />
            : <TrendArea transactions={transactions} currencyCode={currencyCode} />}
        </div>
        <div className="rounded-2xl glass-card p-5 lg:col-span-2">
          <h2 className="mb-4 text-base font-semibold text-slate-900">Cheltuieli pe categorii</h2>
          {summaryLoading
            ? <div className="h-48 animate-pulse rounded-xl bg-slate-200/60" />
            : <CategoryDonut data={summary?.byCategory ?? []} kind="EXPENSE" currencyCode={currencyCode} />}
        </div>
      </div>

      {/* Ultimele tranzactii */}
      <div className="rounded-2xl glass-card p-5">
        <h2 className="mb-4 text-base font-semibold text-slate-900">Ultimele tranzacții</h2>
        <TransactionList
          transactions={recent}
          isLoading={txLoading}
          emptyMessage="Nicio tranzacție în această perioadă."
        />
      </div>
    </div>
  );
};
