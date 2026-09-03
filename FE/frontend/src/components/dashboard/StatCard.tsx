import clsx from 'clsx';
import { formatMoney } from '@/utils/format';

type Tone = 'income' | 'expense' | 'net' | 'dark';

interface StatCardProps {
  label:        string;
  amount:       number;
  currencyCode?: string | null;
  tone?:        Tone;
  icon?:        React.ComponentType<{ className?: string }>;
}

const toneClasses: Record<Tone, { card: string; label: string; amount: string }> = {
  income:  { card: 'glass-card',                  label: 'text-slate-500', amount: 'text-[var(--color-income)]' },
  expense: { card: 'glass-card',                  label: 'text-slate-500', amount: 'text-[var(--color-expense)]' },
  net:     { card: 'glass-card',                  label: 'text-slate-500', amount: 'text-slate-900' },
  dark:    { card: 'bg-slate-900 border-slate-900', label: 'text-slate-400', amount: 'text-white' },
};

export const StatCard: React.FC<StatCardProps> = ({
  label,
  amount,
  currencyCode,
  tone = 'net',
  icon: Icon,
}) => {
  const t = toneClasses[tone];
  return (
    <div className={clsx('rounded-2xl p-5 shadow-sm', t.card)}>
      <div className="flex items-center justify-between">
        <span className={clsx('text-sm font-medium', t.label)}>{label}</span>
        {Icon && (
          <span className={clsx('rounded-lg p-1.5', tone === 'dark' ? 'bg-white/10 text-white' : 'bg-slate-900/5 text-slate-700')}>
            <Icon className="h-4 w-4" />
          </span>
        )}
      </div>
      <p className={clsx('mt-3 text-2xl font-bold tabular-nums', t.amount)}>
        {formatMoney(amount, currencyCode)}
      </p>
    </div>
  );
};
