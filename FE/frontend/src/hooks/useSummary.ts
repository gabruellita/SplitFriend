import { useState, useEffect, useCallback } from 'react';
import { financeApi } from '@/api/financeApi';
import type { TransactionSummary } from '@/types/finance.types';

interface UseSummaryResult {
  summary:   TransactionSummary | null;
  isLoading: boolean;
  error:     string | null;
  refetch:   () => Promise<void>;
}

export const useSummary = (from?: string, to?: string): UseSummaryResult => {
  const [summary, setSummary]   = useState<TransactionSummary | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError]         = useState<string | null>(null);

  const fetchSummary = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await financeApi.getSummary(from, to);
      setSummary(data);
    } catch (err) {
      setError('Nu s-a putut încărca sumarul. Reîncearcă.');
      console.error('useSummary error:', err);
    } finally {
      setIsLoading(false);
    }
  }, [from, to]);

  useEffect(() => {
    void fetchSummary();
  }, [fetchSummary]);

  return { summary, isLoading, error, refetch: fetchSummary };
};
