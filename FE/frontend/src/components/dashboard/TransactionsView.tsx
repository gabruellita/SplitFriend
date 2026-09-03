import { useState } from 'react';
import { Plus, ChevronDown, ChevronUp } from 'lucide-react';

import type { Transaction, TransactionKind, CreateTransactionRequest, CreateRecurringTemplateRequest } from '@/types/finance.types';
import { financeApi } from '@/api/financeApi';
import { useTransactions } from '@/hooks/useTransactions';
import { useCategories } from '@/hooks/useCategories';

import { Button } from '@/components/common/Button';
import { Alert } from '@/components/common/Alert';
import { PeriodFilter } from './PeriodFilter';
import { defaultPeriod, type PeriodValue } from '@/utils/period';
import { TransactionList } from './TransactionList';
import { TransactionFormModal } from './TransactionFormModal';
import { ConfirmDialog } from './ConfirmDialog';
import { CategoryManager } from './CategoryManager';

interface TransactionsViewProps {
  kind:  TransactionKind;
  title: string;
}

export const TransactionsView: React.FC<TransactionsViewProps> = ({ kind, title }) => {
  const [period, setPeriod] = useState<PeriodValue>(defaultPeriod());
  const { categories, refetch: refetchCategories } = useCategories(kind);

  const {
    transactions, isLoading, error, refetch,
    createTransaction, updateTransaction, deleteTransaction,
  } = useTransactions({ from: period.from, to: period.to, kind, categoryId: period.categoryId });

  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing]     = useState<Transaction | null>(null);
  const [toDelete, setToDelete]   = useState<Transaction | null>(null);
  const [deleting, setDeleting]   = useState(false);
  const [showCategories, setShowCategories] = useState(false);

  const openAdd  = () => { setEditing(null); setModalOpen(true); };
  const openEdit = (tx: Transaction) => { setEditing(tx); setModalOpen(true); };

  const handleSubmit = async (body: CreateTransactionRequest) => {
    if (editing) await updateTransaction(editing.id, body);
    else await createTransaction(body);
  };

  const handleSubmitRecurring = async (body: CreateRecurringTemplateRequest) => {
    await financeApi.createRecurringTemplate(body);
    await financeApi.runDueTemplates();   // genereaza imediat tranzactia de azi daca start = azi
    await refetch();
  };

  const confirmDelete = async () => {
    if (!toDelete) return;
    setDeleting(true);
    try {
      await deleteTransaction(toDelete.id);
      setToDelete(null);
    } finally {
      setDeleting(false);
    }
  };

  const addLabel = kind === 'INCOME' ? 'Adaugă venit' : 'Adaugă cheltuială';

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h1 className="text-2xl font-bold text-slate-900">{title}</h1>
        <Button onClick={openAdd}>
          <Plus className="h-4 w-4" /> {addLabel}
        </Button>
      </div>

      <div className="rounded-2xl glass-card p-5">
        <div className="mb-4">
          <PeriodFilter value={period} categories={categories} onChange={setPeriod} />
        </div>

        {error && <Alert type="error" message={error} />}

        <TransactionList
          transactions={transactions}
          isLoading={isLoading}
          onEdit={openEdit}
          onDelete={setToDelete}
          emptyMessage={`Nicio ${kind === 'INCOME' ? 'intrare' : 'cheltuială'} în această perioadă.`}
        />
      </div>

      {/* Sectiune categorii (colapsabila) */}
      <div className="rounded-2xl glass-card p-5">
        <button type="button" onClick={() => setShowCategories(s => !s)}
          className="flex w-full items-center justify-between text-left cursor-pointer">
          <span className="text-base font-semibold text-slate-900">
            Categorii {kind === 'INCOME' ? 'de venituri' : 'de cheltuieli'}
          </span>
          {showCategories ? <ChevronUp className="h-5 w-5 text-slate-500" /> : <ChevronDown className="h-5 w-5 text-slate-500" />}
        </button>
        {showCategories && (
          <div className="mt-4">
            <CategoryManager kind={kind} onChange={refetchCategories} />
          </div>
        )}
      </div>

      <TransactionFormModal
        open={modalOpen}
        kind={kind}
        categories={categories}
        initial={editing}
        onSubmit={handleSubmit}
        onSubmitRecurring={handleSubmitRecurring}
        onClose={() => setModalOpen(false)}
      />

      <ConfirmDialog
        open={!!toDelete}
        title="Ștergere tranzacție"
        message="Tranzacția va fi anulată (soft-delete). Continui?"
        loading={deleting}
        onConfirm={() => void confirmDelete()}
        onCancel={() => setToDelete(null)}
      />
    </div>
  );
};
