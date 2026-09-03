// src/components/groups/tabs/ExpensesTab.tsx
import { useState } from 'react';
import { Plus, Receipt } from 'lucide-react';
import { useGroupContext } from '@/context/GroupContext';
import { useGroupExpenses } from '@/hooks/useGroupExpenses';
import type { GroupExpense } from '@/types/group.types';
import { ExpenseRow } from '@/components/groups/ExpenseRow';
import { ExpenseDetailModal } from '@/components/groups/ExpenseDetailModal';
import { AddExpenseModal } from '@/components/groups/AddExpenseModal';
import { Button } from '@/components/common/Button';
import { Alert } from '@/components/common/Alert';
import { Spinner } from '@/components/common/Spinner';

export const ExpensesTab: React.FC = () => {
  const { group } = useGroupContext();
  const { expenses, isLoading, error, create, cancel } = useGroupExpenses(group.id);
  const [addOpen, setAddOpen] = useState(false);
  const [selected, setSelected] = useState<GroupExpense | null>(null);

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="font-semibold text-slate-900">Cheltuieli</h2>
        <Button onClick={() => setAddOpen(true)}><Plus className="h-4 w-4" aria-hidden="true" /> Cheltuială</Button>
      </div>

      {error && <Alert type="error" message={error} />}

      {isLoading ? (
        <div className="flex justify-center py-12"><Spinner size="lg" /></div>
      ) : expenses.length === 0 ? (
        <div className="glass-card flex flex-col items-center rounded-2xl px-6 py-12 text-center">
          <Receipt className="mb-3 h-8 w-8 text-slate-400" aria-hidden="true" />
          <p className="text-sm text-slate-500">Nicio cheltuială încă. Adaugă prima cheltuială de grup.</p>
        </div>
      ) : (
        <div className="divide-y divide-slate-900/10 overflow-hidden rounded-2xl glass-card">
          {expenses.map(e => <ExpenseRow key={e.id} expense={e} onClick={() => setSelected(e)} />)}
        </div>
      )}

      <AddExpenseModal open={addOpen} onClose={() => setAddOpen(false)}
        onSubmit={async (body) => { await create(body); }} />
      <ExpenseDetailModal expense={selected} onClose={() => setSelected(null)}
        onCancel={async (id) => { await cancel(id); }} />
    </div>
  );
};
