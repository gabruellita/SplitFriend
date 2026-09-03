import { useState, useEffect, useCallback } from 'react';
import { financeApi } from '@/api/financeApi';
import type {
  Transaction,
  TransactionFilters,
  CreateTransactionRequest,
  UpdateTransactionRequest,
} from '@/types/finance.types';

interface UseTransactionsResult {
  transactions:      Transaction[];
  isLoading:         boolean;
  error:             string | null;
  refetch:           () => Promise<void>;
  createTransaction: (body: CreateTransactionRequest) => Promise<void>;
  updateTransaction: (id: number, body: UpdateTransactionRequest) => Promise<void>;
  deleteTransaction: (id: number) => Promise<void>;
}

export const useTransactions = (filters: TransactionFilters = {}): UseTransactionsResult => {
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [isLoading, setIsLoading]       = useState(true);
  const [error, setError]               = useState<string | null>(null);

  // Serializam filtrele ca sa avem o dependenta stabila pentru useCallback/useEffect.
  const filterKey = JSON.stringify(filters);

  const fetchTransactions = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await financeApi.getTransactions(JSON.parse(filterKey) as TransactionFilters);
      setTransactions(data);
    } catch (err) {
      setError('Nu s-au putut încărca tranzacțiile. Reîncearcă.');
      console.error('useTransactions error:', err);
    } finally {
      setIsLoading(false);
    }
  }, [filterKey]);

  useEffect(() => {
    void fetchTransactions();
  }, [fetchTransactions]);

  const createTransaction = useCallback(async (body: CreateTransactionRequest) => {
    await financeApi.createTransaction(body);
    await fetchTransactions();
  }, [fetchTransactions]);

  const updateTransaction = useCallback(async (id: number, body: UpdateTransactionRequest) => {
    await financeApi.updateTransaction(id, body);
    await fetchTransactions();
  }, [fetchTransactions]);

  const deleteTransaction = useCallback(async (id: number) => {
    await financeApi.deleteTransaction(id);
    await fetchTransactions();
  }, [fetchTransactions]);

  return {
    transactions,
    isLoading,
    error,
    refetch: fetchTransactions,
    createTransaction,
    updateTransaction,
    deleteTransaction,
  };
};
