// src/hooks/useGroupBalances.ts
import { useState, useEffect, useCallback } from 'react';
import { groupsApi } from '@/api/groupsApi';
import type { GroupBalance } from '@/types/group.types';

interface UseGroupBalancesResult {
  balances:  GroupBalance[];
  isLoading: boolean;
  error:     string | null;
  refetch:   () => Promise<void>;
}

export const useGroupBalances = (groupId: number): UseGroupBalancesResult => {
  const [balances, setBalances] = useState<GroupBalance[]>([]);
  const [isLoading, setLoading] = useState(true);
  const [error, setError]       = useState<string | null>(null);

  const fetchAll = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      setBalances(await groupsApi.getBalances(groupId));
    } catch (err) {
      setError('Nu s-au putut încărca balanțele. Reîncearcă.');
      console.error('useGroupBalances error:', err);
    } finally {
      setLoading(false);
    }
  }, [groupId]);

  useEffect(() => { void fetchAll(); }, [fetchAll]);

  return { balances, isLoading, error, refetch: fetchAll };
};
