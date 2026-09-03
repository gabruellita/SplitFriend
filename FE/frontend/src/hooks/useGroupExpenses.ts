// src/hooks/useGroupExpenses.ts
import { useState, useEffect, useCallback } from 'react';
import { groupsApi } from '@/api/groupsApi';
import type { GroupExpense, CreateGroupExpenseRequest } from '@/types/group.types';

interface UseGroupExpensesResult {
  expenses:  GroupExpense[];
  isLoading: boolean;
  error:     string | null;
  refetch:   () => Promise<void>;
  create:    (body: CreateGroupExpenseRequest) => Promise<void>;
  cancel:    (expenseId: number) => Promise<void>;
}

export const useGroupExpenses = (groupId: number): UseGroupExpensesResult => {
  const [expenses, setExpenses] = useState<GroupExpense[]>([]);
  const [isLoading, setLoading] = useState(true);
  const [error, setError]       = useState<string | null>(null);

  const fetchAll = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      setExpenses(await groupsApi.getExpenses(groupId));
    } catch (err) {
      setError('Nu s-au putut încărca cheltuielile. Reîncearcă.');
      console.error('useGroupExpenses error:', err);
    } finally {
      setLoading(false);
    }
  }, [groupId]);

  useEffect(() => { void fetchAll(); }, [fetchAll]);

  const create = useCallback(async (body: CreateGroupExpenseRequest) => {
    await groupsApi.createExpense(groupId, body);
    void fetchAll();   // refresh în fundal; o eroare aici NU înseamnă că salvarea a eșuat
  }, [groupId, fetchAll]);

  const cancel = useCallback(async (expenseId: number) => {
    await groupsApi.cancelExpense(groupId, expenseId);
    void fetchAll();   // idem
  }, [groupId, fetchAll]);

  return { expenses, isLoading, error, refetch: fetchAll, create, cancel };
};
