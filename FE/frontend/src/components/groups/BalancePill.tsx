// src/components/groups/BalancePill.tsx
import clsx from 'clsx';
import { formatMoney } from '@/utils/format';

interface BalancePillProps {
  amount:        number;        // + ți se datorează, − datorezi, 0 neutru
  currencyCode?: string | null;
  className?:    string;
}

/** Afișează un sold cu semantică de culoare verde/roșu/neutru. */
export const BalancePill: React.FC<BalancePillProps> = ({ amount, currencyCode, className }) => {
  const rounded = Math.round(amount * 100) / 100;
  const tone = rounded > 0 ? 'text-emerald-600' : rounded < 0 ? 'text-rose-600' : 'text-slate-500';
  const sign = rounded > 0 ? '+' : '';
  return (
    <span className={clsx('font-semibold tabular-nums', tone, className)}>
      {sign}{formatMoney(rounded, currencyCode)}
    </span>
  );
};
