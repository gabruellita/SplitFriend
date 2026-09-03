// src/components/groups/ExpenseDetailModal.tsx
import { useState } from 'react';
import { X, Trash2 } from 'lucide-react';
import type { GroupExpense } from '@/types/group.types';
import { useGroupContext } from '@/context/GroupContext';
import { formatMoney, formatDate } from '@/utils/format';
import { Button } from '@/components/common/Button';
import { Alert } from '@/components/common/Alert';
import { ConvertedAmount } from '@/components/groups/ConvertedAmount';
import { useUserCurrency } from '@/hooks/useUserCurrency';

interface ExpenseDetailModalProps {
  expense:  GroupExpense | null;
  onCancel: (expenseId: number) => Promise<void>;
  onClose:  () => void;
}

export const ExpenseDetailModal: React.FC<ExpenseDetailModalProps> = ({ expense, onCancel, onClose }) => {
  const { nameOf, currentUserId } = useGroupContext();
  const [canceling, setCanceling]     = useState(false);
  const [cancelError, setCancelError] = useState<string | null>(null);

  // Moneda vizualizatorului (codul real, rezolvat din preferredCurrencyId — vezi
  // useUserCurrency) — pentru echivalentul live. Apelat înainte de early-return (rules of hooks).
  const viewerCurrency = useUserCurrency() ?? undefined;

  if (!expense) return null;

  const canCancel = expense.status === 'OPEN' &&
    (expense.paidByUserId === currentUserId);

  const handleCancel = async () => {
    setCanceling(true);
    setCancelError(null);
    try {
      await onCancel(expense.id);
      onClose();
    } catch {
      setCancelError('Nu s-a putut anula cheltuiala. Reîncearcă.');
    } finally {
      setCanceling(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-slate-900/50" onClick={onClose} aria-hidden="true" />
      <div role="dialog" aria-modal="true" aria-label={expense.title}
        className="relative max-h-[90vh] w-full max-w-md overflow-y-auto rounded-2xl bg-white p-6 shadow-xl">
        <button type="button" onClick={onClose} aria-label="Închide"
          className="absolute right-4 top-4 rounded-lg p-1 text-slate-400 hover:bg-slate-100 hover:text-slate-700">
          <X className="h-5 w-5" />
        </button>
        <h2 className="mb-1 text-lg font-semibold text-slate-900">{expense.title}</h2>
        <p className="mb-4 text-sm text-slate-500">
          {formatMoney(expense.amount, expense.currencyCode)} · plătit de {nameOf(expense.paidByUserId)} · {formatDate(expense.expenseDate)}
        </p>

        <h3 className="mb-2 text-sm font-medium text-slate-700">Împărțire ({expense.splitType})</h3>
        <ul className="divide-y divide-slate-100 rounded-xl border border-slate-200">
          {expense.splits.map(s => (
            <li key={s.userId} className="flex items-center justify-between px-3 py-2 text-sm">
              <span className="truncate text-slate-700">
                {nameOf(s.userId)}{s.userId === currentUserId && <span className="ml-1 text-xs text-brand-600">(Tu)</span>}
              </span>
              <span className="flex items-center gap-2">
                <span className="flex flex-col items-end gap-0.5">
                  <span className="tabular-nums text-slate-900">{formatMoney(s.owedAmount, expense.currencyCode)}</span>
                  <ConvertedAmount amount={s.owedAmount} from={expense.currencyCode} to={viewerCurrency} />
                </span>
                {s.isSettled && <span className="rounded-full bg-emerald-100 px-2 py-0.5 text-xs text-emerald-700">achitat</span>}
              </span>
            </li>
          ))}
        </ul>

        <p className="mt-3 rounded-lg bg-brand-50 px-3 py-2 text-xs text-brand-700">
          Această cheltuială apare automat și în registrul personal al fiecărui participant.
        </p>

        {canCancel && (
          <div className="mt-4 flex flex-col items-end gap-2">
            {cancelError && <Alert type="error" message={cancelError} />}
            <Button variant="danger" loading={canceling} disabled={canceling} onClick={handleCancel}>
              <Trash2 className="h-4 w-4" aria-hidden="true" /> Anulează cheltuiala
            </Button>
          </div>
        )}
      </div>
    </div>
  );
};
