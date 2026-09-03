import { useMemo } from 'react';
import {
  AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend,
} from 'recharts';
import { LineChart as LineIcon } from 'lucide-react';
import type { Transaction } from '@/types/finance.types';
import { formatMoney } from '@/utils/format';

interface TrendAreaProps {
  transactions: Transaction[];
  currencyCode?: string | null;
}

interface DayPoint { date: string; income: number; expense: number; }

export const TrendArea: React.FC<TrendAreaProps> = ({ transactions, currencyCode }) => {
  const data = useMemo<DayPoint[]>(() => {
    const byDay = new Map<string, DayPoint>();
    for (const tx of transactions) {
      const point = byDay.get(tx.transactionDate) ?? { date: tx.transactionDate, income: 0, expense: 0 };
      if (tx.kind === 'INCOME') point.income += tx.amount;
      else point.expense += tx.amount;
      byDay.set(tx.transactionDate, point);
    }
    return [...byDay.values()].sort((a, b) => a.date.localeCompare(b.date));
  }, [transactions]);

  if (data.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-12 text-center text-slate-500">
        <LineIcon className="mb-2 h-8 w-8" />
        <p className="text-sm">Nu există date pentru această perioadă.</p>
      </div>
    );
  }

  const dayLabel = (iso: string) => iso.slice(8, 10); // ziua din "YYYY-MM-DD"

  return (
    <div className="h-64 w-full">
      <ResponsiveContainer width="100%" height="100%">
        <AreaChart data={data} margin={{ top: 8, right: 8, bottom: 0, left: -12 }}>
          <defs>
            <linearGradient id="gIncome" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0%" stopColor="var(--color-income)" stopOpacity={0.4} />
              <stop offset="100%" stopColor="var(--color-income)" stopOpacity={0} />
            </linearGradient>
            <linearGradient id="gExpense" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0%" stopColor="var(--color-expense)" stopOpacity={0.4} />
              <stop offset="100%" stopColor="var(--color-expense)" stopOpacity={0} />
            </linearGradient>
          </defs>
          <CartesianGrid strokeDasharray="3 3" stroke="#e2e8f0" vertical={false} />
          <XAxis dataKey="date" tickFormatter={dayLabel} tick={{ fontSize: 12, fill: '#64748b' }} />
          <YAxis tick={{ fontSize: 12, fill: '#64748b' }} width={48} />
          <Tooltip formatter={(v) => formatMoney(Number(v), currencyCode)} />
          <Legend formatter={(v) => (v === 'income' ? 'Venituri' : 'Cheltuieli')} />
          <Area type="monotone" dataKey="income" stroke="var(--color-income)" strokeWidth={2}
            fill="url(#gIncome)" />
          <Area type="monotone" dataKey="expense" stroke="var(--color-expense)" strokeWidth={2}
            strokeDasharray="5 3" fill="url(#gExpense)" />
        </AreaChart>
      </ResponsiveContainer>
    </div>
  );
};
