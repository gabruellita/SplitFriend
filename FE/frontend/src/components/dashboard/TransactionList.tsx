import { useState, useMemo } from 'react';
import clsx from 'clsx';
import { Pencil, Trash2, ArrowUp, ArrowDown, Inbox } from 'lucide-react';
import type { Transaction } from '@/types/finance.types';
import { formatMoney, formatDate } from '@/utils/format';

type SortKey = 'transactionDate' | 'amount';
type SortDir = 'asc' | 'desc';

interface TransactionListProps {
  transactions: Transaction[];
  isLoading?:   boolean;
  onEdit?:      (tx: Transaction) => void;
  onDelete?:    (tx: Transaction) => void;
  emptyMessage?: string;
}

export const TransactionList: React.FC<TransactionListProps> = ({
  transactions,
  isLoading = false,
  onEdit,
  onDelete,
  emptyMessage = 'Nicio tranzacție în această perioadă.',
}) => {
  const [sortKey, setSortKey] = useState<SortKey>('transactionDate');
  const [sortDir, setSortDir] = useState<SortDir>('desc');

  const sorted = useMemo(() => {
    const arr = [...transactions];
    arr.sort((a, b) => {
      const cmp = sortKey === 'amount'
        ? a.amount - b.amount
        : a.transactionDate.localeCompare(b.transactionDate);
      return sortDir === 'asc' ? cmp : -cmp;
    });
    return arr;
  }, [transactions, sortKey, sortDir]);

  const toggleSort = (key: SortKey) => {
    if (key === sortKey) setSortDir(d => (d === 'asc' ? 'desc' : 'asc'));
    else { setSortKey(key); setSortDir('desc'); }
  };

  const ariaSort = (key: SortKey): 'ascending' | 'descending' | 'none' =>
    sortKey !== key ? 'none' : sortDir === 'asc' ? 'ascending' : 'descending';

  const renderSortIcon = (col: SortKey) => {
    if (sortKey !== col) return null;
    return sortDir === 'asc'
      ? <ArrowUp className="inline h-3.5 w-3.5" />
      : <ArrowDown className="inline h-3.5 w-3.5" />;
  };

  if (isLoading) {
    return (
      <div className="space-y-2" aria-busy="true">
        {Array.from({ length: 5 }).map((_, i) => (
          <div key={i} className="h-12 animate-pulse rounded-xl bg-slate-200/60" />
        ))}
      </div>
    );
  }

  if (sorted.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-12 text-center text-slate-500">
        <Inbox className="mb-2 h-8 w-8" />
        <p className="text-sm">{emptyMessage}</p>
      </div>
    );
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-left text-sm">
        <thead>
          <tr className="border-b border-slate-900/10 text-xs uppercase tracking-wide text-slate-500">
            <th className="py-2 pr-3" aria-sort={ariaSort('transactionDate')}>
              <button type="button" onClick={() => toggleSort('transactionDate')}
                className="inline-flex items-center gap-1 font-medium hover:text-slate-900 cursor-pointer">
                Data {renderSortIcon('transactionDate')}
              </button>
            </th>
            <th className="py-2 pr-3 font-medium">Categorie</th>
            <th className="hidden py-2 pr-3 font-medium sm:table-cell">Descriere</th>
            <th className="py-2 pr-3 text-right" aria-sort={ariaSort('amount')}>
              <button type="button" onClick={() => toggleSort('amount')}
                className="inline-flex items-center gap-1 font-medium hover:text-slate-900 cursor-pointer">
                Sumă {renderSortIcon('amount')}
              </button>
            </th>
            {(onEdit || onDelete) && <th className="py-2 pl-3 text-right font-medium">Acțiuni</th>}
          </tr>
        </thead>
        <tbody>
          {sorted.map(tx => {
            const isIncome = tx.kind === 'INCOME';
            return (
              <tr key={tx.id} className="border-b border-slate-900/5 last:border-0 hover:bg-slate-900/[0.02]">
                <td className="py-3 pr-3 whitespace-nowrap text-slate-600">{formatDate(tx.transactionDate)}</td>
                <td className="py-3 pr-3">
                  <span className="inline-flex items-center gap-2">
                    <span
                      className="h-2.5 w-2.5 shrink-0 rounded-full"
                      style={{ backgroundColor: isIncome ? 'var(--color-income)' : 'var(--color-expense)' }}
                      aria-hidden="true"
                    />
                    <span className="text-slate-800">{tx.categoryName ?? 'Fără categorie'}</span>
                  </span>
                </td>
                <td className="hidden py-3 pr-3 text-slate-500 sm:table-cell">{tx.description ?? '—'}</td>
                <td className={clsx('py-3 pr-3 text-right font-semibold tabular-nums',
                  isIncome ? 'text-[var(--color-income)]' : 'text-[var(--color-expense)]')}>
                  {isIncome ? '+' : '−'}{formatMoney(tx.amount, tx.currencyCode)}
                </td>
                {(onEdit || onDelete) && (
                  <td className="py-3 pl-3 text-right whitespace-nowrap">
                    {onEdit && (
                      <button type="button" onClick={() => onEdit(tx)} aria-label="Editează"
                        className="rounded-lg p-1.5 text-slate-500 hover:bg-slate-100 hover:text-slate-900 cursor-pointer">
                        <Pencil className="h-4 w-4" />
                      </button>
                    )}
                    {onDelete && (
                      <button type="button" onClick={() => onDelete(tx)} aria-label="Șterge"
                        className="rounded-lg p-1.5 text-slate-500 hover:bg-rose-50 hover:text-rose-600 cursor-pointer">
                        <Trash2 className="h-4 w-4" />
                      </button>
                    )}
                  </td>
                )}
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
};
