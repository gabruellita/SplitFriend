// src/hooks/useGroupPayments.ts
import { useState, useEffect, useCallback } from 'react';
import { groupsApi } from '@/api/groupsApi';
import type { Payment, CreatePaymentRequest } from '@/types/group.types';

interface UseGroupPaymentsResult {
  payments:  Payment[];
  isLoading: boolean;
  error:     string | null;
  refetch:   () => Promise<void>;
  create:    (body: CreatePaymentRequest) => Promise<void>;
}

export const useGroupPayments = (groupId: number): UseGroupPaymentsResult => {
  const [payments, setPayments] = useState<Payment[]>([]);
  const [isLoading, setLoading] = useState(true);
  const [error, setError]       = useState<string | null>(null);

  const fetchAll = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      setPayments(await groupsApi.getPayments(groupId));
    } catch (err) {
      setError('Nu s-au putut încărca plățile. Reîncearcă.');
      console.error('useGroupPayments error:', err);
    } finally {
      setLoading(false);
    }
  }, [groupId]);

  useEffect(() => { void fetchAll(); }, [fetchAll]);

  const create = useCallback(async (body: CreatePaymentRequest) => {
    // POST-ul e awaitat — orice eroare de la server se propagă spre caller.
    await groupsApi.createPayment(groupId, body);
    // refresh în fundal; o eroare aici NU înseamnă că plata a eșuat
    void fetchAll();
  }, [groupId, fetchAll]);

  return { payments, isLoading, error, refetch: fetchAll, create };
};
