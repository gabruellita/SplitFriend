// src/components/groups/ExpenseRow.tsx
import { Receipt } from 'lucide-react';
import type { GroupExpense } from '@/types/group.types';
import { useGroupContext } from '@/context/GroupContext';
import { formatMoney, formatDate } from '@/utils/format';

const STATUS_STYLE: Record<string, string> = {
  OPEN:     'bg-amber-100 text-amber-700',
  SETTLED:  'bg-emerald-100 text-emerald-700',
  CANCELED: 'bg-slate-200 text-slate-500',
};

interface ExpenseRowProps {
  expense: GroupExpense;
  onClick: () => void;
}

export const ExpenseRow: React.FC<ExpenseRowProps> = ({ expense, onClick }) => {
  const { nameOf } = useGroupContext();
  return (
    <button type="button" onClick={onClick}
      className="flex w-full items-center gap-3 px-4 py-3 text-left transition hover:bg-slate-900/5 focus:outline-none focus:ring-2 focus:ring-brand-500 cursor-pointer">
      <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-xl bg-slate-100 text-slate-500">
        <Receipt className="h-4 w-4" aria-hidden="true" />
      </span>
      <div className="min-w-0 flex-1">
        <p className={`truncate font-medium text-slate-900 ${expense.status === 'CANCELED' ? 'line-through' : ''}`}>
          {expense.title}
        </p>
        <p className="truncate text-sm text-slate-500">
          {nameOf(expense.paidByUserId)} · {formatDate(expense.expenseDate)}
        </p>
      </div>
      <div className="flex shrink-0 flex-col items-end gap-1">
        <span className="font-semibold tabular-nums text-slate-900">{formatMoney(expense.amount, expense.currencyCode)}</span>
        <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${STATUS_STYLE[expense.status] ?? ''}`}>
          {expense.status}
        </span>
      </div>
    </button>
  );
};
